using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEditorView : VisualElement, IDisposable {
        public readonly EditorWindow editorWindow;
        public WorldGraphGraphView graphView { get; private set; }
        private readonly WorldGraph graph;

        private readonly WorldGraphEditor _GraphEditor;
        private string _AssetName;

        private Vector2 scrollPos;

        private Blackboard exposedPropertiesBlackboard;
        private Blackboard inspectorBlackboard;
        private MasterPreviewView masterPreviewView;
        private GenericMenu exposedPropertiesItemMenu;

        public WorldGraphEditorToolbar toolbar { get; }
        private WorldGraphSearcherProvider m_SearchWindowProvider;

        private EdgeConnectorListener edgeConnectorListener;

        const string k_PreviewWindowLayoutKey = "TN.WorldGraph.PreviewWindowLayout";
        private WindowDockingLayout previewDockingLayout => m_PreviewDockingLayout;
        private readonly WindowDockingLayout m_PreviewDockingLayout = new WindowDockingLayout {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
        };

        private const string k_InspectorWindowLayoutKey = "TN.WorldGraph.InspectorWindowLayout";
        private WindowDockingLayout inspectorDockingLayout => m_InspectorDockingLayout;
        private readonly WindowDockingLayout m_InspectorDockingLayout = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        private const string k_BlackboardWindowLayoutKey = "TN.WorldGraph.ExposedPropertiesWindowLayout";
        private WindowDockingLayout blackboardDockingLayout => m_BlackboardDockingLayout;
        private readonly WindowDockingLayout m_BlackboardDockingLayout = new WindowDockingLayout {
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

        private readonly BlackboardFieldManipulator blackboardFieldManipulator;

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
                graphView = new WorldGraphGraphView(graph) {name = "GraphView", viewDataKey = "MaterialGraphView"};
                content.Add(graphView);

                DeserializeWindowLayouts(ref m_PreviewDockingLayout, k_PreviewWindowLayoutKey);
                DeserializeWindowLayouts(ref m_InspectorDockingLayout, k_InspectorWindowLayoutKey);
                DeserializeWindowLayouts(ref m_BlackboardDockingLayout, k_BlackboardWindowLayoutKey);

                CreateMasterPreview();
                CreateInspectorBlackboard();
                CreateExposedPropertiesBlackboard();

                UpdateSubWindowsVisibility();

                graphView.graphViewChanged = GraphViewChanged;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }
            Add(content);

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, graphView, CreateNode);
            graphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != this.editorWindow) return;
                var displayPosition = (c.screenMousePosition - this.editorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(this.editorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            edgeConnectorListener = new EdgeConnectorListener(editorWindow, m_SearchWindowProvider);
            blackboardFieldManipulator = new BlackboardFieldManipulator(this);

            LoadGraph();
            graphView.RegisterPortCallbacks();
        }


        private void LoadGraph() {
            // ------------------ Create Base SceneHandle ------------------
            if (graph.IsEmpty) {
                graph.CreateSubAsset(typeof(BaseHandle));
            }

            // ------------------ Create nodes for every scene handle ------------------
            foreach (var graphNode in graph.sceneHandles.Select(sceneHandle =>
                new WorldGraphNodeView(graphView, sceneHandle, edgeConnectorListener))) {
                graphView.AddElement(graphNode);
            }

            // ------------------ Create edges for the nodes ------------------
            foreach (var parent1 in graph.sceneHandles) {
                IEnumerable<SceneHandle> children = WorldGraph.GetChildren(parent1);
                foreach (var child in children) {
                    WorldGraphNodeView baseView = (WorldGraphNodeView) graphView.GetNodeByGuid(parent1.guid);
                    WorldGraphNodeView targetView = (WorldGraphNodeView) graphView.GetNodeByGuid(child.guid);
                    var edge = baseView?.output.ConnectTo(targetView?.input);
                    graphView.AddElement(edge);
                }
            }

            // ------------------ Create Parameters in Blackboard + Create Parameter Nodes ------------------
            foreach (var exposedParam in graph.allParameters) {
                CreateBlackboardField(exposedParam);
                if (exposedParam.Displayed) {
                    CreateParameterGraphNode(exposedParam, exposedParam.Position);
                }
            }

            // ------------------ Connect Parameter Nodes to the respective Parameter Ports ------------------
            foreach (var sceneHandle in graph.sceneHandles) {
                WorldGraphNodeView baseView = (WorldGraphNodeView) graphView.GetNodeByGuid(sceneHandle.guid);
                List<WorldGraphPort> ports = baseView.inputContainer.Query<WorldGraphPort>().ToList();
                foreach (var parameter in sceneHandle.allParameters) {
                    ParameterPropertyNodeView paramView = (ParameterPropertyNodeView) graphView.GetNodeByGuid(parameter.GUID);
                    foreach (var port in ports) {
                        if (parameter.ConnectedPortGUID == port.PortData.GUID) {
                            var edge = paramView.output.ConnectTo(port);
                            graphView.AddElement(edge);
                        }
                    }
                }
            }
        }

        public void CreateNode(Type type, Vector2 position) {
            SceneHandle node = graph.CreateSubAsset(type);
            node.position = position;

            var graphNode = new WorldGraphNodeView(graphView, node, edgeConnectorListener);
            graphNode.OnSelected();
            graphView.AddElement(graphNode);
        }


        private GraphViewChange GraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                switch (elem) {
                    case WorldGraphNodeView nodeView:
                        graph.RemoveSubAsset(nodeView.sceneHandle);
                        break;
                    case BlackboardField blackboardField:
                        var Edges = graphView.edges.ToList();
                        ExposedParameter exposedParameter = (ExposedParameter) blackboardField.userData;

                        // Delete if node is present on GraphView
                        if (exposedParameter.Displayed) {
                            var paramNode = graphView.GetNodeByGuid(exposedParameter.GUID);

                            // Also Delete Port -- RemovePort function handles the rest 
                            if (exposedParameter.ConnectedPortGUID != null) {
                                Edge connectedEdge = Edges.Find(edge =>
                                    ((WorldGraphPort) edge.input).PortData.GUID == exposedParameter.ConnectedPortGUID);
                                var paramPort = ((WorldGraphPort) connectedEdge.input);
                                paramPort.RemoveParameterPort(paramPort.PortData);
                            }

                            graphView.RemoveElement(paramNode);
                        }

                        graph.RemoveParameter(exposedParameter);
                        break;
                    case ParameterPropertyNodeView parameterNodeView:
                        parameterNodeView.parameter.Displayed = false;
                        break;
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edgeView => { });
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
                    // using var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos);
                    // scrollPos = scrollViewScope.scrollPosition;
                    // _GraphEditor!.OnInspectorGUI();
                }));
            }
            graphView.Add(inspectorBlackboard);
        }

        private void CreateExposedPropertiesBlackboard() {
            exposedPropertiesBlackboard = new Blackboard(graphView) {title = "Exposed Properties", subTitle = "WorldGraph"};
            {
                exposedPropertiesBlackboard.Add(new BlackboardSection {title = "Exposed Variables"});
                exposedPropertiesBlackboard.editTextRequested = (_blackboard, element, newValue) => {
                    var param = (ExposedParameter) ((BlackboardField) element).userData;
                    var paramNode = graphView.graphElements
                        .OfType<ParameterPropertyNodeView>()
                        .ToList()
                        .Find(x => x.parameter == param);

                    param.Name = newValue;
                    if (paramNode != null) paramNode.output.portName = newValue;

                    ((BlackboardField) element).text = newValue;
                };

                exposedPropertiesItemMenu = new GenericMenu();

                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false,
                    () => { CreateBlackboardField(CreateExposedParameter(ParameterType.String)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false,
                    () => { CreateBlackboardField(CreateExposedParameter(ParameterType.Float)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false,
                    () => { CreateBlackboardField(CreateExposedParameter(ParameterType.Int)); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false,
                    () => { CreateBlackboardField(CreateExposedParameter(ParameterType.Bool)); });
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedPropertiesBlackboard.addItemRequested += _ => exposedPropertiesItemMenu.ShowAsContext();
            }

            graphView.Add(exposedPropertiesBlackboard);
        }

        private ExposedParameter CreateExposedParameter(ParameterType type) {
            return graph.CreateParameter(type);
        }

        private BlackboardField CreateBlackboardField(ExposedParameter parameter) {
            var field = new BlackboardField {
                userData = parameter,
                text = $"{parameter.Name}",
                typeText = parameter.ParameterType.ToString(),
                icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null
            };
            exposedPropertiesBlackboard.Add(field);
            return field;
        }

        public void CreateParameterGraphNode(ExposedParameter parameter, Vector2 position) {
            parameter.Position = position;
            parameter.Displayed = true;

            var outputPort = new PortData {
                PortColor = new Color(0.52f, 0.89f, 0.91f),
                PortDirection = "Output",
                PortType = PortType.Parameter,
            };

            var outputPortView = new WorldGraphPort(outputPort, edgeConnectorListener);
            var parameterNodeView = new ParameterPropertyNodeView(parameter, outputPortView);

            graphView.AddElement(parameterNodeView);
        }


        #region Serialize Window Layouts

        private static void DeserializeWindowLayouts(ref WindowDockingLayout layout, string layoutKey) {
            string serializedLayout = EditorUserSettings.GetConfigValue(layoutKey);
            if (!string.IsNullOrEmpty(serializedLayout)) {
                layout = JsonUtility.FromJson<WindowDockingLayout>(serializedLayout) ?? new WindowDockingLayout();
            }
        }

        private void ApplySerializedWindowLayouts(GeometryChangedEvent evt) {
            UnregisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            ApplySerializedLayout(inspectorBlackboard, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            ApplySerializedLayout(exposedPropertiesBlackboard, blackboardDockingLayout, k_BlackboardWindowLayoutKey);
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

        #endregion

        public void Dispose() {
            if (graphView != null) {
                toolbar.Dispose();
                blackboardFieldManipulator.target.RemoveManipulator(blackboardFieldManipulator);
                graphView.graphElements.OfType<IWorldGraphNodeView>().ToList().ForEach(node => node.Dispose());
                graphView.nodeCreationRequest = null;
                graphView = null;
            }

            if (m_SearchWindowProvider == null) return;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;
        }
    }

}