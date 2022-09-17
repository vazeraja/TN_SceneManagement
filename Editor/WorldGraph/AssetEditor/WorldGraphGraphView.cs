using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView, IDisposable {
        private readonly EditorWindow editorWindow;
        private readonly WorldGraph graph;

        public WorldGraphSearcherProvider m_SearchWindowProvider;
        private EdgeConnectorListener edgeConnectorListener;

        public Blackboard inspectorBlackboard;
        public Blackboard exposedParametersBlackboard;

        private VisualElement _RootElement;
        private Label titleLabel;
        private ScrollView inspectorContentContainer;
        private IMGUIContainer GUIContainer;
        private WorldGraphEditor graphEditor;

        private List<ParameterPropertyNodeView> ParameterNodeViews => graphElements.OfType<ParameterPropertyNodeView>().ToList();

        public void Dispose() {
            graphElements.OfType<IWorldGraphNodeView>().ToList().ForEach(node => node.Dispose());

            nodeCreationRequest = null;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;

            _RootElement = null;
            titleLabel = null;
            inspectorContentContainer = null;
            GUIContainer = null;

            inspectorBlackboard = null;
            exposedParametersBlackboard = null;
        }

        public WorldGraphGraphView(EditorWindow editorWindow, WorldGraph graph) {
            this.editorWindow = editorWindow;
            this.graph = graph;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            SetupZoom(0.05f, 8);
            AddToClassList("drop-area");

            _RootElement = new VisualElement();
            var visualTreeAsset = Resources.Load<VisualTreeAsset>($"UXML/InspectorContainer");
            _RootElement.styleSheets.Add(Resources.Load<StyleSheet>("UXML/InspectorContainer"));
            visualTreeAsset.CloneTree(_RootElement);
            titleLabel = _RootElement.Q<Label>("title-label");
            inspectorContentContainer = _RootElement.Q<ScrollView>("content-container");

            graphEditor = UnityEditor.Editor.CreateEditor(graph) as WorldGraphEditor;

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, this, CreateNode);
            nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != editorWindow) return;
                var displayPosition = (c.screenMousePosition - this.editorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(editorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };
            edgeConnectorListener = new EdgeConnectorListener(editorWindow, m_SearchWindowProvider);
        }

        public void Initialize() {
            ShowGraphSettings();

            // ------------------ Create Base Node ------------------
            if (!graph.sceneHandles.Any()) {
                SceneHandle baseHandle = graph.CreateSubAsset(typeof(BaseHandle));
                baseHandle.WorldGraph = graph;
            }

            // ------------------ Create Nodes ------------------
            foreach (var sceneHandle in graph.sceneHandles) {
                CreateGraphNode(sceneHandle);
            }
            
            // ------------------ Connect Nodes ------------------
            foreach (WorldGraphEdge graphEdge in from edge in graph.edges
                let outputView = (WorldGraphNodeView) GetNodeByGuid(edge.outputNodeGUID)
                let inputView = (WorldGraphNodeView) GetNodeByGuid(edge.inputNodeGUID)
                select outputView.output.ConnectTo<WorldGraphEdge>(inputView.input)) {
                AddElement(graphEdge);
            }

            // ------------------ Create Parameters ------------------
            foreach (var exposedParam in graph.allParameters) {
                CreateBlackboardField(exposedParam);
                if (exposedParam.Displayed) {
                    CreateParameterGraphNode(exposedParam, false);
                }
            }
            
            // ------------------ Connect Parameter Nodes  ------------------
            foreach (WorldGraphEdge edge in from sceneHandle in graph.sceneHandles
                let baseView = (WorldGraphNodeView) GetNodeByGuid(sceneHandle.GUID)
                let ports = baseView.inputContainer.Query<WorldGraphPort>().ToList()
                from parameter in sceneHandle.allParameters
                let paramView = (ParameterPropertyNodeView) GetNodeByGuid(parameter.GUID)
                from port in ports
                where parameter.ConnectedPortGUID == port.PortData.GUID
                select ((WorldGraphPort) paramView.output).ConnectTo<WorldGraphEdge>(port)) {
                AddElement(edge);
            }
        }

        private void CreateNode(Type type, Vector2 position) {
            SceneHandle node = graph.CreateSubAsset(type);
            node.WorldGraph = graph;
            node.Position = position;
            CreateGraphNode(node);
        }

        private void CreateGraphNode(SceneHandle sceneHandle) {
            var graphNode = new WorldGraphNodeView(this, sceneHandle, edgeConnectorListener);
            AddElement(graphNode);
        }

        public void CreateParameterGraphNode(ExposedParameter parameter, bool isDropped) {
            var outputPort = new PortData {
                PortColor = new Color(0.52f, 0.89f, 0.91f),
                PortDirection = "Output",
                PortCapacity = "Single",
                PortType = PortType.Parameter,
            };

            var outputPortView = new WorldGraphPort(outputPort, edgeConnectorListener);
            if (isDropped) InitializePortBehavior(outputPortView);
            var parameterNodeView = new ParameterPropertyNodeView(parameter, outputPortView);

            AddElement(parameterNodeView);
        }

        public void CreateBlackboardField(ExposedParameter parameter) {
            var blackboardField = new WGBlackboardField(parameter);
            exposedParametersBlackboard.Add(blackboardField);
        }

        public void DrawInspector(Object objectToDisplay) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            switch (objectToDisplay) {
                case SceneHandle sceneHandle:
                    DrawProperties(sceneHandle, $"{sceneHandle.HandleName} Node", true);
                    break;
                case FloatParameterField floatParameterField:
                    DrawProperties(floatParameterField, $"{floatParameterField.Name} Parameter");
                    break;
                case IntParameterField intParameterField:
                    DrawProperties(intParameterField, $"{intParameterField.Name} Parameter");
                    break;
                case BoolParameterField boolParameterField:
                    DrawProperties(boolParameterField, $"{boolParameterField.Name} Parameter");
                    break;
                case StringParameterField stringParameterField:
                    DrawProperties(stringParameterField, $"{stringParameterField.Name} Parameter");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(objectToDisplay));
            }
        }

        private void DrawProperties(Object obj, string label, bool isSceneHandle = false) {
            var serializedParameter = new SerializedObject(obj);
            var fieldInfos = WGHelper.GetFieldInfosWithAttribute(obj, typeof(WGInspectableAttribute));

            titleLabel.text = label;
            GUIContainer = new IMGUIContainer(() => {
                if (!isSceneHandle) {
                    serializedParameter.Update();
                    foreach (var field in fieldInfos) {
                        var prop = serializedParameter.FindProperty(field.Name);
                        EditorGUILayout.PropertyField(prop);
                    }

                    serializedParameter.ApplyModifiedProperties();
                }
                else {
                    var editor = UnityEditor.Editor.CreateEditor((SceneHandle) obj);
                    editor.OnInspectorGUI();
                }
            });
            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }

        public void ShowGraphSettings() {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            titleLabel.text = "Graph Settings";
            GUIContainer = new IMGUIContainer(() => { graphEditor.OnInspectorGUI(); });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }

        public void ShowTransitionInformation(SceneHandle output, SceneHandle input) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();
            titleLabel.text = $"{output.HandleName} ----> {input.HandleName}";

            GUIContainer = new IMGUIContainer(() => { });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }

        public void RegisterPortCallbacks() {
            ports.OfType<WorldGraphPort>().ToList().ForEach(InitializePortBehavior);
        }

        public void InitializePortBehavior(WorldGraphPort worldGraphPort) {
            // ------------ Dragging an edge disconnects both ports ------------
            worldGraphPort.OnConnected += (node, port, edge) => {
                WorldGraphPort outputPort = (WorldGraphPort) edge.output;
                WorldGraphPort inputPort = (WorldGraphPort) edge.input;

                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (port.PortData.PortType) {
                    case PortType.Default when port.direction == Direction.Output: {
                        var output = (WorldGraphNodeView) outputPort.node;
                        var input = (WorldGraphNodeView) inputPort.node;

                        graph.AddChild(output.sceneHandle, input.sceneHandle);
                        graph.edges.Add(new EdgeData {
                            outputNodeGUID = output.sceneHandle.GUID,
                            outputNode = output.sceneHandle,
                            inputNodeGUID = input.sceneHandle.GUID,
                            inputNode = input.sceneHandle
                        });
                        break;
                    }
                    case PortType.Parameter when node is WorldGraphNodeView nodeView: {
                        Debug.Log($"{node.name} Parameter Port Connected");
                        ExposedParameter parameter = ((ParameterPropertyNodeView) outputPort.node).parameter;
                        nodeView.sceneHandle.AddParameter(parameter);
                        port.PortData.Parameter = parameter;
                        break;
                    }
                    case PortType.Parameter when node is ParameterPropertyNodeView paramNodeView:
                        Debug.Log("Parameter Port Connected");
                        paramNodeView.parameter.ConnectedPortGUID = inputPort.PortData.GUID;
                        break;
                }
            };
            worldGraphPort.OnDisconnected += (node, port, edge) => {
                WorldGraphPort outputPort = (WorldGraphPort) edge.output;
                WorldGraphPort inputPort = (WorldGraphPort) edge.input;


                // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
                switch (port.PortData.PortType) {
                    case PortType.Default when port.direction == Direction.Output: {
                        var output = (WorldGraphNodeView) outputPort.node;
                        var input = (WorldGraphNodeView) inputPort.node;

                        graph.RemoveChild(output.sceneHandle, input.sceneHandle);
                        var edgeToRemove = graph.edges.Find(e =>
                            e.outputNodeGUID == output.sceneHandle.GUID && e.inputNodeGUID == input.sceneHandle.GUID);
                        graph.edges.Remove(edgeToRemove);

                        break;
                    }
                    case PortType.Parameter when node is WorldGraphNodeView nodeView:
                        Debug.Log($"{node.name} Parameter Port Disconnected");
                        nodeView.sceneHandle.RemoveParameter(port.PortData.Parameter);
                        port.PortData.Parameter = null;
                        break;
                    case PortType.Parameter when node is ParameterPropertyNodeView propertyNodeView:
                        Debug.Log("Parameter Port Disconnected");
                        propertyNodeView.parameter.ConnectedPortGUID = null;
                        break;
                }
            };
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    ((WorldGraphPort) endPort).PortData.OwnerNodeGUID != ((WorldGraphPort) startPort).PortData.OwnerNodeGUID &&
                    ((WorldGraphPort) endPort).PortData.PortDirection != ((WorldGraphPort) startPort).PortData.PortDirection &&
                    ((WorldGraphPort) endPort).PortData.PortType == ((WorldGraphPort) startPort).PortData.PortType)
                .ToList();
        }

        public void RegisterCompleteObjectUndo(string name) {
            Undo.RegisterCompleteObjectUndo(graph, name);
        }
    }

}