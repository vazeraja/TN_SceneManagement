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

        public WorldGraphEditorToolbar toolbar { get; }
        private WorldGraphSearcherProvider m_SearchWindowProvider;

        private EdgeConnectorListener edgeConnectorListener;

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

                toolbar.saveRequested += () => { SaveAsset(); };
                graphView.graphViewChanged = GraphViewChanged;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, graphView, CreateNode);
            graphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != this.editorWindow) return;
                var displayPosition = (c.screenMousePosition - this.editorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(this.editorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            edgeConnectorListener = new EdgeConnectorListener(this.editorWindow, m_SearchWindowProvider);

            LoadGraph();

            Add(content);
        }

        private void LoadGraph() {
            if (graph.sceneHandles.Count == 0) {
                graph.CreateSubAsset(typeof(BaseHandle));
            }

            foreach (var sceneHandle in graph.sceneHandles) {
                CreateGraphNode(sceneHandle);
            }

            foreach (var parent in graph.sceneHandles) {
                var children = WorldGraph.GetChildren(parent);
                foreach (var child in children) {
                    WorldGraphNodeView baseView = graphView.GetNodeByGuid(parent.guid) as WorldGraphNodeView;
                    WorldGraphNodeView targetView = graphView.GetNodeByGuid(child.guid) as WorldGraphNodeView;
                    var edge = baseView?.output.ConnectTo(targetView?.input);
                    graphView.AddElement(edge);
                }
            }

            // foreach (var nodeLink in graph.edges) {
            //     WorldGraphNodeView baseView = graphView.GetNodeByGuid(nodeLink.BaseNodeGUID) as WorldGraphNodeView;
            //     WorldGraphNodeView targetView = graphView.GetNodeByGuid(nodeLink.TargetNodeGUID) as WorldGraphNodeView;
            //     var edge = baseView?.output.ConnectTo(targetView?.input);
            //     graphView.AddElement(edge);
            // }

            foreach (var exposedParam in graph.stringParameters) {
                AddProperty(ParameterType.String, exposedParam);
            }

            foreach (var exposedParam in graph.floatParameters) {
                AddProperty(ParameterType.Float, exposedParam);
            }

            foreach (var exposedParam in graph.intParameters) {
                AddProperty(ParameterType.Int, exposedParam);
            }

            foreach (var exposedParam in graph.boolParameters) {
                AddProperty(ParameterType.Bool, exposedParam);
            }
        }

        private bool SaveAsset() {
            graph.edges.Clear();

            var Edges = graphView.edges.ToList();
            var connectedSockets = Edges.Where(x => x.input.node != null).ToList();

            foreach (var edge in connectedSockets) {
                var outputNode = edge.output.node as WorldGraphNodeView;
                var inputNode = edge.input.node as WorldGraphNodeView;

                graph.edges.Add(new EdgeData {
                    BaseSceneHandle = outputNode?.sceneHandle,
                    BaseNodeGUID = outputNode?.viewDataKey,
                    TargetSceneHandle = inputNode?.sceneHandle,
                    TargetNodeGUID = inputNode?.viewDataKey
                });
            }

            EditorUtility.SetDirty(graph);
            return true;
        }

        private void CreateNode(Type type, Vector2 position) {
            SceneHandle node = graph.CreateSubAsset(type);
            node.position = position;

            var graphNode = new WorldGraphNodeView(graphView, node, edgeConnectorListener);
            graphNode.OnSelected();
            graphView.AddElement(graphNode);
        }

        private void CreateGraphNode(SceneHandle node) {
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
                    case WorldGraphEdge edgeView:
                        var output = edgeView.output.node as WorldGraphNodeView;
                        var input = edgeView.input.node as WorldGraphNodeView;
                        graph.RemoveChild(output?.sceneHandle, input?.sceneHandle);
                        break;
                    case BlackboardField blackboardField:
                        ExposedParameter fieldData = blackboardField.userData as ExposedParameter;
                        switch (fieldData) {
                            case StringParameterField stringParameterField:
                                graph.stringParameters.Remove(stringParameterField);
                                break;
                            case FloatParameterField floatParameterField:
                                graph.floatParameters.Remove(floatParameterField);
                                break;
                            case IntParameterField intParameterField:
                                graph.intParameters.Remove(intParameterField);
                                break;
                            case BoolParameterField boolParameterField:
                                graph.boolParameters.Remove(boolParameterField);
                                break;
                        }

                        var ancestor = WGEditorGUI.GetFirstAncestorWhere(blackboardField, i => i.name == "b_field");
                        exposedPropertiesBlackboard.Remove(ancestor);
                        break;
                }
            });

            graphViewChange.edgesToCreate?.ForEach(edgeView => {
                var output = edgeView.output.node as WorldGraphNodeView;
                var input = edgeView.input.node as WorldGraphNodeView;
                graph.AddChild(output?.sceneHandle, input?.sceneHandle);
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
                exposedPropertiesBlackboard.Add(new BlackboardSection {
                    title = "Exposed Variables"
                });
                exposedPropertiesBlackboard.editTextRequested = (_blackboard, element, newValue) => {
                    var param = (ExposedParameter) ((BlackboardField) element).userData;
                    switch (((ExposedParameter) ((BlackboardField) element).userData).ParameterType) {
                        case ParameterType.String:
                            graph.stringParameters.Find(x => x == param).Name = newValue;
                            break;
                        case ParameterType.Float:
                            graph.floatParameters.Find(x => x == param).Name = newValue;
                            break;
                        case ParameterType.Int:
                            graph.intParameters.Find(x => x == param).Name = newValue;
                            break;
                        case ParameterType.Bool:
                            graph.boolParameters.Find(x => x == param).Name = newValue;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    ((BlackboardField) element).text = newValue;
                };

                exposedPropertiesItemMenu = new GenericMenu();
                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => { AddProperty(ParameterType.String); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => { AddProperty(ParameterType.Float); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => { AddProperty(ParameterType.Int); });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => { AddProperty(ParameterType.Bool); });
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedPropertiesBlackboard.addItemRequested += b => exposedPropertiesItemMenu.ShowAsContext();
            }

            graphView.Add(exposedPropertiesBlackboard);
        }

        private void AddProperty(ParameterType type, ExposedParameter parameter = null) {
            ExposedParameter parameterToCreate = null;
            VisualElement valueField;

            switch (type) {
                case ParameterType.String:
                    if (parameter == null) {
                        parameterToCreate = graph.CreateParameter(ParameterType.String);
                        valueField = new TextField("Value:") {value = "localPropertyValue"};
                    }
                    else {
                        valueField = new TextField("Value:") {value = ((StringParameterField) parameter).Value};
                    }

                    ((TextField) valueField).RegisterValueChangedCallback(evt => {
                        if (parameter == null) {
                            graph.stringParameters.Find(x => x == parameterToCreate).Value = evt.newValue;
                        }
                        else {
                            graph.stringParameters.Find(x => x == parameter).Value = evt.newValue;
                        }
                    });
                    break;
                case ParameterType.Float:
                    if (parameter == null) {
                        parameterToCreate = graph.CreateParameter(ParameterType.Float);
                        valueField = new FloatField("Value:") {value = 5f};
                    }
                    else {
                        valueField = new FloatField("Value:") {value = ((FloatParameterField) parameter).Value};
                    }

                    ((FloatField) valueField).RegisterValueChangedCallback(evt => {
                        if (parameter == null) {
                            graph.floatParameters.Find(x => x == parameterToCreate).Value = evt.newValue;
                        }
                        else {
                            graph.floatParameters.Find(x => x == parameter).Value = evt.newValue;
                        }
                    });
                    break;
                case ParameterType.Int:
                    if (parameter == null) {
                        parameterToCreate = graph.CreateParameter(ParameterType.Int);
                        valueField = new IntegerField("Value:") {value = 5};
                    }
                    else {
                        valueField = new IntegerField("Value:") {value = ((IntParameterField) parameter).Value};
                    }

                    ((IntegerField) valueField).RegisterValueChangedCallback(evt => {
                        if (parameter == null) {
                            graph.intParameters.Find(x => x == parameterToCreate).Value = evt.newValue;
                        }
                        else {
                            graph.intParameters.Find(x => x == parameter).Value = evt.newValue;
                        }
                    });
                    break;
                case ParameterType.Bool:
                    if (parameter == null) {
                        parameterToCreate = graph.CreateParameter(ParameterType.Bool);
                        valueField = new Toggle("Value:") {value = true};
                    }
                    else {
                        valueField = new Toggle("Value:") {value = ((BoolParameterField) parameter).Value};
                    }

                    ((Toggle) valueField).RegisterValueChangedCallback(evt => {
                        if (parameter == null) {
                            graph.boolParameters.Find(x => x == parameterToCreate).Value = evt.newValue;
                        }
                        else {
                            graph.boolParameters.Find(x => x == parameter).Value = evt.newValue;
                        }
                    });
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }

            var container = new VisualElement {name = "b_field"};
            BlackboardField field;

            if (parameter == null) {
                field = new BlackboardField {
                    userData = parameterToCreate,
                    text = $"{parameterToCreate.Name}",
                    typeText = parameterToCreate.ParameterType.ToString(),
                    icon = parameterToCreate.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null
                };
                container.Add(field);
            }
            else {
                field = new BlackboardField {
                    userData = parameter,
                    text = $"{parameter.Name}",
                    typeText = parameter.ParameterType.ToString(),
                    icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null
                };
                container.Add(field);
            }


            var row = new BlackboardRow(field, valueField);
            container.Add(row);

            exposedPropertiesBlackboard.Add(container);
        }

        #region Serialize Window Layouts

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

        #endregion

        public void Dispose() {
            if (graphView != null) {
                toolbar.Dispose();
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