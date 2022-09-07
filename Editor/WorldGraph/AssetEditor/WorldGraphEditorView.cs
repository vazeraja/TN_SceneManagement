using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEditorView : VisualElement, IDisposable {
        private readonly EditorWindow _EditorWindow;
        public WorldGraphGraphView GraphView { get; private set; }

        private readonly WorldGraph _Graph;
        private readonly WorldGraphEditor _GraphEditor;
        private string _AssetName;

        private Vector2 scrollPos;

        private Blackboard exposedPropertiesBlackboard;
        private Blackboard inspectorBlackboard;
        private MasterPreviewView masterPreviewView;
        private GenericMenu exposedPropertiesItemMenu;

        private WorldGraphSearcherProvider m_SearchWindowProvider;

        private EdgeConnectorListener edgeConnectorListener;

        public WorldGraphEditorToolbar toolbar { get; }

        private List<IWorldGraphNodeView> nodeViews => GraphView.graphElements.OfType<IWorldGraphNodeView>().ToList();

        const string k_PreviewWindowLayoutKey = "TN.WorldGraph.PreviewWindowLayout";

        private WindowDockingLayout previewDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
        };

        private const string k_InspectorWindowLayoutKey = "TN.WorldGraph.InspectorWindowLayout";

        private WindowDockingLayout inspectorDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        private const string k_PropertiesWindowLayoutKey = "TN.WorldGraph.ExposedPropertiesWindowLayout";

        private WindowDockingLayout exposedPropertiesDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = true,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        public string assetName {
            get => _AssetName;
            set {
                _AssetName = value;
                inspectorBlackboard.title = _AssetName + " Inspector";
            }
        }

        public WorldGraphEditorView(EditorWindow editorWindow, WorldGraph graph, string graphName) {
            _EditorWindow = editorWindow;
            _Graph = graph;
            _AssetName = graphName;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEditorView"));

            _GraphEditor = UnityEditor.Editor.CreateEditor(graph) as WorldGraphEditor;

            toolbar = new WorldGraphEditorToolbar {changeCheck = UpdateSubWindowsVisibility};
            var toolbarGUI = new IMGUIContainer(() => { toolbar.OnGUI(); });
            Add(toolbarGUI);

            var content = new VisualElement {name = "content"};
            {
                GraphView = new WorldGraphGraphView(graph) {
                    name = "GraphView", viewDataKey = "MaterialGraphView"
                };
                GraphView.styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
                GraphView.AddManipulator(new ContentDragger());
                GraphView.AddManipulator(new SelectionDragger());
                GraphView.AddManipulator(new RectangleSelector());
                GraphView.AddManipulator(new ClickSelector());
                GraphView.SetupZoom(0.05f, 8);

                content.Add(GraphView);

                string serializedPreview = EditorUserSettings.GetConfigValue(k_PreviewWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedPreview)) {
                    previewDockingLayout = JsonUtility.FromJson<WindowDockingLayout>(serializedPreview) ?? new WindowDockingLayout();
                }

                string serializedInspector = EditorUserSettings.GetConfigValue(k_InspectorWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedInspector)) {
                    inspectorDockingLayout =
                        JsonUtility.FromJson<WindowDockingLayout>(serializedInspector) ?? new WindowDockingLayout();
                }

                string serializedBlackboard = EditorUserSettings.GetConfigValue(k_PropertiesWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedBlackboard)) {
                    exposedPropertiesDockingLayout =
                        JsonUtility.FromJson<WindowDockingLayout>(serializedBlackboard) ?? new WindowDockingLayout();
                }

                CreateMasterPreview();
                CreateInspectorBlackboard();
                CreateExposedPropertiesBlackboard();

                UpdateSubWindowsVisibility();

                GraphView.graphViewChanged = GraphViewChanged;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, GraphView, (type, position) => { CreateNode(type, position); });
            GraphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != _EditorWindow) return;
                var displayPosition = (c.screenMousePosition - _EditorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(_EditorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            edgeConnectorListener = new EdgeConnectorListener(_EditorWindow, m_SearchWindowProvider);

            AddNodes();

            Add(content);
        }

        private void AddNodes() {
            if (_Graph.sceneHandles.Count == 0) {
                _Graph.CreateSubAsset(typeof(BaseHandle));
            }

            foreach (var sceneHandle in _Graph.sceneHandles) {
                CreateGraphNode(sceneHandle);
            }

            foreach (var edge in from p in _Graph.sceneHandles
                let children = WorldGraph.GetChildren(p)
                from c in children
                let parentView = GraphView.GetNodeByGuid(p.guid) as WorldGraphNodeView
                let childView = GraphView.GetNodeByGuid(c.guid) as WorldGraphNodeView
                where parentView?.sceneHandle is not BaseHandle || childView?.sceneHandle != null
                select parentView?.output.ConnectTo(childView?.input)) {
                GraphView.AddElement(edge);
            }
        }

        private SceneHandle CreateNode(Type type, Vector2 position) {
            SceneHandle node = _Graph.CreateSubAsset(type);
            node.position = position;
            CreateGraphNode(node);
            return node;
        }

        private void CreateGraphNode(SceneHandle node) {
            var graphNode = new WorldGraphNodeView(GraphView, node, edgeConnectorListener);
            graphNode.OnSelected();
            GraphView.AddElement(graphNode);
        }

        private GraphViewChange GraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                switch (elem) {
                    case IWorldGraphNodeView nodeView:
                        _Graph.RemoveSubAsset(nodeView.sceneHandle);
                        break;
                    case WorldGraphEdge edge:
                        var parentView = edge.output.node as IWorldGraphNodeView;
                        var childView = edge.input.node as IWorldGraphNodeView;
                        _Graph.RemoveChild(parentView?.sceneHandle, childView?.sceneHandle);
                        break;
                    case BlackboardField blackboardField:
                        var ancestor = WGEditorGUI.GetFirstAncestorWhere(blackboardField, i => i.name == "b_field");
                        exposedPropertiesBlackboard.Remove(ancestor);
                        break;
                    // case ReanimatorGroup group:
                    //     SaveToGraphSaveData();
                    //     break;
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge => {
                var parentView = edge.output.node as IWorldGraphNodeView;
                var childView = edge.input.node as IWorldGraphNodeView;

                _Graph.AddChild(parentView?.sceneHandle, childView?.sceneHandle);
            });

            return graphViewChange;
        }

        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView(GraphView, _EditorWindow, _Graph) {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            GraphView.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void CreateInspectorBlackboard() {
            inspectorBlackboard = new Blackboard(GraphView) {title = "Inspector", subTitle = "WorldGraph"};
            {
                inspectorBlackboard.Add(new IMGUIContainer(() => {
                    // ReSharper disable once ConvertToUsingDeclaration
                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                        scrollPos = scrollViewScope.scrollPosition;
                        _GraphEditor!.OnInspectorGUI();
                    }
                }));
            }
            GraphView.Add(inspectorBlackboard);
        }

        private void CreateExposedPropertiesBlackboard() {
            exposedPropertiesBlackboard = new Blackboard(GraphView) {title = "Exposed Properties", subTitle = "WorldGraph"};
            {
                exposedPropertiesBlackboard.Add(new BlackboardSection {
                    title = "Exposed Variables"
                });
                exposedPropertiesBlackboard.editTextRequested = (_blackboard, element, newValue) => {
                    string oldPropertyName = ((BlackboardField) element).text;
                    ((BlackboardField) element).text = newValue;
                };

                exposedPropertiesItemMenu = new GenericMenu();
                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => { AddProperty(typeof(string)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => { AddProperty(typeof(float)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => { AddProperty(typeof(int)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => { AddProperty(typeof(bool)); });
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedPropertiesBlackboard.addItemRequested += b => exposedPropertiesItemMenu.ShowAsContext();
            }

            GraphView.Add(exposedPropertiesBlackboard);
        }

        private void AddProperty(Type type) {
            var container = new VisualElement {name = "b_field"};
            BlackboardField field = new BlackboardField {
                text = $"New {type.Name}", typeText = type.Name,
                icon = Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed")
            };
            container.Add(field);

            VisualElement valueField = null;
            if (type == typeof(string)) {
                valueField = new TextField("Value:") {value = "localPropertyValue"};
                ((TextField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(float)) {
                valueField = new FloatField("Value:") {value = 5.5f};
                ((FloatField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(int)) {
                valueField = new IntegerField("Value:") {value = 5};
                ((IntegerField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(bool)) {
                valueField = new Toggle("Value:") {};
                ((Toggle) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            var row = new BlackboardRow(field, valueField);
            container.Add(row);

            exposedPropertiesBlackboard.Add(container);
        }


        private void ApplySerializedWindowLayouts(GeometryChangedEvent evt) {
            UnregisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            ApplySerializedLayout(inspectorBlackboard, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            ApplySerializedLayout(exposedPropertiesBlackboard, exposedPropertiesDockingLayout, k_PropertiesWindowLayoutKey);
            ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
        }

        private void ApplySerializedLayout(VisualElement target, WindowDockingLayout layout, string layoutKey) {
            layout.ApplySize(target);
            layout.ApplyPosition(target);

            target.RegisterCallback<GeometryChangedEvent>((evt) => {
                layout.CalculateDockingCornerAndOffset(target.layout, GraphView.layout);
                layout.ClampToParentWindow();

                string serializedWindowLayout = JsonUtility.ToJson(layout);
                EditorUserSettings.SetConfigValue(layoutKey, serializedWindowLayout);
            });
        }

        private void UpdateSubWindowsVisibility() {
            exposedPropertiesBlackboard.visible = toolbar.m_UserViewSettings.isBlackboardVisible;
            inspectorBlackboard.visible = toolbar.m_UserViewSettings.isInspectorVisible;
            masterPreviewView.visible = toolbar.m_UserViewSettings.isPreviewVisible;
        }

        public void Dispose() {
            if (GraphView != null) {
                toolbar.Dispose();
                nodeViews.ForEach(node => node.Dispose());
                GraphView.nodeCreationRequest = null;
                GraphView = null;
            }

            if (m_SearchWindowProvider == null) return;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;
        }
    }

}