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
        private readonly EditorWindow editorWindow;
        public WorldGraphGraphView graphView { get; private set; }
        private readonly WorldGraph graph;

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

        private List<IWorldGraphNodeView> nodeViews => graphView.graphElements.OfType<IWorldGraphNodeView>().ToList();

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
            this.editorWindow = editorWindow;
            this.graph = graph;
            _AssetName = graphName;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEditorView"));

            _GraphEditor = UnityEditor.Editor.CreateEditor(graph) as WorldGraphEditor;

            toolbar = new WorldGraphEditorToolbar {changeCheck = UpdateSubWindowsVisibility};
            var toolbarGUI = new IMGUIContainer(() => { toolbar.OnGUI(); });
            Add(toolbarGUI);

            var content = new VisualElement {name = "content"};
            {
                graphView = new WorldGraphGraphView(graph) {
                    name = "GraphView", viewDataKey = "MaterialGraphView"
                };
                graphView.styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
                graphView.AddManipulator(new ContentDragger());
                graphView.AddManipulator(new SelectionDragger());
                graphView.AddManipulator(new RectangleSelector());
                graphView.AddManipulator(new ClickSelector());
                graphView.SetupZoom(0.05f, 8);

                content.Add(graphView);

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

                graphView.graphViewChanged = GraphViewChanged;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, graphView, (type, position) => { CreateNode(type, position); });
            graphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != this.editorWindow) return;
                var displayPosition = (c.screenMousePosition - this.editorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(this.editorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            edgeConnectorListener = new EdgeConnectorListener(this.editorWindow, m_SearchWindowProvider);

            AddNodes();

            Add(content);
        }

        private void AddNodes() {
            if (graph.sceneHandles.Count == 0) {
                graph.CreateSubAsset(typeof(BaseHandle));
            }

            foreach (var sceneHandle in graph.sceneHandles) {
                CreateGraphNode(sceneHandle);
            }

            foreach (var edge in from p in graph.sceneHandles
                let children = WorldGraph.GetChildren(p)
                from c in children
                let parentView = graphView.GetNodeByGuid(p.guid) as WorldGraphNodeView
                let childView = graphView.GetNodeByGuid(c.guid) as WorldGraphNodeView
                where parentView?.sceneHandle is not BaseHandle || childView?.sceneHandle != null
                select parentView?.output.ConnectTo(childView?.input)) {
                graphView.AddElement(edge);
            }
        }

        private SceneHandle CreateNode(Type type, Vector2 position) {
            SceneHandle node = graph.CreateSubAsset(type);
            node.position = position;
            CreateGraphNode(node);
            return node;
        }

        private void CreateGraphNode(SceneHandle node) {
            var graphNode = new WorldGraphNodeView(graphView, node, edgeConnectorListener);
            graphNode.OnSelected();
            graphView.AddElement(graphNode);
        }

        private GraphViewChange GraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                switch (elem) {
                    case IWorldGraphNodeView nodeView:
                        graph.RemoveSubAsset(nodeView.sceneHandle);
                        break;
                    case WorldGraphEdge edge:
                        var parentView = edge.output.node as IWorldGraphNodeView;
                        var childView = edge.input.node as IWorldGraphNodeView;
                        graph.RemoveChild(parentView?.sceneHandle, childView?.sceneHandle);
                        break;
                    case BlackboardField blackboardField:
                        ExposedParameter fieldData = blackboardField.userData as ExposedParameter;
                        switch (fieldData) {
                            case StringParameterField data:
                                graph.stringParameters.Remove(data);
                                break;
                            case FloatParameterField data:
                                graph.floatParameters.Remove(data);
                                break;
                            case IntParameterField data:
                                graph.intParameters.Remove(data);
                                break;
                            case BoolParameterField data:
                                graph.boolParameters.Remove(data);
                                break;
                        }

                        var ancestor = WGEditorGUI.GetFirstAncestorWhere(blackboardField, i => i.name == "b_field");
                        exposedPropertiesBlackboard.Remove(ancestor);
                        break;
                    case Group group:
                        break;
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge => {
                var parentView = edge.output.node as IWorldGraphNodeView;
                var childView = edge.input.node as IWorldGraphNodeView;

                graph.AddChild(parentView?.sceneHandle, childView?.sceneHandle);
            });

            return graphViewChange;
        }

        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView(graphView, editorWindow, graph) {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            graphView.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void CreateInspectorBlackboard() {
            inspectorBlackboard = new Blackboard(graphView) {title = "Inspector", subTitle = "WorldGraph"};
            {
                inspectorBlackboard.Add(new IMGUIContainer(() => {
                    using var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos);
                    scrollPos = scrollViewScope.scrollPosition;
                    _GraphEditor!.OnInspectorGUI();
                }));
            }
            graphView.Add(inspectorBlackboard);
        }

        private void CreateExposedPropertiesBlackboard() {
            exposedPropertiesBlackboard = new Blackboard(graphView) {title = "Exposed Properties", subTitle = "WorldGraph"};
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

            graphView.Add(exposedPropertiesBlackboard);
        }

        private void AddProperty(Type type) {
            ExposedParameter parameter = null;
            string typeName = null;
            
            VisualElement valueField = null;

            if (type == typeof(string)) {
                parameter = graph.CreateStringParameter();
                typeName = "String";
                
                valueField = new TextField("Value:") {value = "localPropertyValue"};
                ((TextField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(float)) {
                parameter = graph.CreateFloatParameter();
                typeName = "Float";
                
                valueField = new FloatField("Value:") {value = 5f};
                ((FloatField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(int)) {
                parameter = graph.CreateIntParameter();
                typeName = "Int";
                
                valueField = new IntegerField("Value:") {value = 5};
                ((IntegerField) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }

            if (type == typeof(bool)) {
                parameter = graph.CreateBoolParameter();
                typeName = "Bool";
                
                valueField = new Toggle("Value:") {value = true};
                ((Toggle) valueField).RegisterValueChangedCallback(evt => { Debug.Log("changed"); });
            }
            
            var container = new VisualElement {name = "b_field"};
            BlackboardField field = null;

            field = new BlackboardField {
                text = $"New {typeName}",
                typeText = typeName,
                userData = parameter,
                icon = parameter is {Exposed: true} ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null
            };
            container.Add(field);

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
                layout.CalculateDockingCornerAndOffset(target.layout, graphView.layout);
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
            if (graphView != null) {
                toolbar.Dispose();
                nodeViews.ForEach(node => node.Dispose());
                graphView.nodeCreationRequest = null;
                graphView = null;
            }

            if (m_SearchWindowProvider == null) return;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;
        }
    }

}