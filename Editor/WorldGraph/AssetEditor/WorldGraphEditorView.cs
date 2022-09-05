using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEditorView : VisualElement, IDisposable {
        private readonly EditorWindow _EditorWindow;
        private WorldGraphGraphView _GraphView;

        private readonly WorldGraph _Graph;
        private readonly WorldGraphEditor _GraphEditor;
        private string _AssetName;

        private Vector2 scrollPos;

        private Blackboard exposedPropertiesBlackboard;
        private Blackboard inspectorBlackboard;
        private MasterPreviewView masterPreviewView;
        private GenericMenu exposedPropertiesItemMenu;

        private BaseEdgeConnectorListener connectorListener;
        private WGSearcherProvider m_SearchWindowProvider;
        private SearcherWindow searcherWindow;

        public WorldGraphEditorToolbar toolbar { get; set; }

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

        public WorldGraphGraphView GraphView => _GraphView;

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
                _GraphView = new WorldGraphGraphView(graph) {
                    name = "GraphView", viewDataKey = "MaterialGraphView"
                };
                _GraphView.styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
                _GraphView.AddManipulator(new ContentDragger());
                _GraphView.AddManipulator(new SelectionDragger());
                _GraphView.AddManipulator(new RectangleSelector());
                _GraphView.AddManipulator(new ClickSelector());
                _GraphView.SetupZoom(0.05f, 8);

                content.Add(_GraphView);

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
                
                _GraphView.graphViewChanged = GraphViewChanged;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WGSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, _GraphView, SearchWindowItemSelected);
            _GraphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != _EditorWindow) return;
                var displayPosition = (c.screenMousePosition - _EditorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(_EditorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            AddNodes();

            Add(content);
        }

        private void AddNodes() {
            if (_Graph.sceneHandles.Count == 0) {
                _Graph.CreateSubAsset(typeof(BaseHandle));
                EditorUtility.SetDirty(_Graph);
                AssetDatabase.SaveAssets();
            }
            
            _Graph.sceneHandles.ForEach((sceneHandle) => {
                var node = new WGNodeView(_GraphView, sceneHandle);
                _GraphView.AddElement(node);
            });
        }
        private void SearchWindowItemSelected(Type type) {
            SceneHandle sceneHandle = _Graph.CreateSubAsset(type);
            var node = new WGNodeView(_GraphView, sceneHandle);

            _GraphView.AddElement(node);
            Debug.Log("Node Count: " + _GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());
        }

        private GraphViewChange GraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                switch (elem) {
                    case IWorldGraphNodeView nodeDisplay:
                        _Graph.RemoveSubAsset(nodeDisplay.sceneHandle);
                        break;
                    // case Edge edge: 
                    //     ReanimatorGraphNode parent = edge.output.node as ReanimatorGraphNode;
                    //     ReanimatorGraphNode child = edge.input.node as ReanimatorGraphNode;
                    //     graph.RemoveChild(parent?.node, child?.node);
                    //     break;
                    // case ReanimatorGroup group:
                    //     SaveToGraphSaveData();
                    //     break;
                }
            });
            graphViewChange.edgesToCreate?.ForEach(edge => {
                // ReanimatorGraphNode parent = edge.output.node as ReanimatorGraphNode;
                // ReanimatorGraphNode child = edge.input.node as ReanimatorGraphNode;
                //
                // graph.AddChild(parent?.node, child?.node);
                // SaveToGraphSaveData();
            });

            return graphViewChange;
        }

        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView(_GraphView, _EditorWindow, _Graph) {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            _GraphView.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void CreateInspectorBlackboard() {
            inspectorBlackboard = new Blackboard(_GraphView) {title = "Inspector", subTitle = "WorldGraph"};
            {
                inspectorBlackboard.Add(new IMGUIContainer(() => {
                    // ReSharper disable once ConvertToUsingDeclaration
                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                        scrollPos = scrollViewScope.scrollPosition;
                        _GraphEditor!.OnInspectorGUI();
                    }
                }));
            }
            _GraphView.Add(inspectorBlackboard);
        }

        private void CreateExposedPropertiesBlackboard() {
            exposedPropertiesBlackboard = new Blackboard(_GraphView) {title = "Exposed Properties", subTitle = "WorldGraph"};
            {
                exposedPropertiesItemMenu = new GenericMenu();
                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => Debug.Log("String"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => Debug.Log("Float"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => Debug.Log("Int"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => Debug.Log("Bool"));
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedPropertiesBlackboard.addItemRequested += b => exposedPropertiesItemMenu.ShowAsContext();
            }

            _GraphView.Add(exposedPropertiesBlackboard);
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
                layout.CalculateDockingCornerAndOffset(target.layout, _GraphView.layout);
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
            if (_GraphView != null) {
                toolbar.Dispose();

                // Get all nodes and dispose
                // Debug.Log("Node Count: " + m_GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());
                foreach (IWorldGraphNodeView node in _GraphView.graphElements.OfType<IWorldGraphNodeView>()) {
                    // _Graph.RemoveSubAsset(node.sceneHandle);
                    _GraphView.RemoveElement(node.gvNode);

                    node.Dispose();
                }
                // Debug.Log("Node Count: " + m_GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());

                _GraphView.nodeCreationRequest = null;
                _GraphView = null;
            }

            if (m_SearchWindowProvider == null) return;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;
        }
    }

}