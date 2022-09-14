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

        private readonly VisualElement _RootElement;
        private readonly Label titleLabel;
        private readonly ScrollView inspectorContentContainer;
        private IMGUIContainer GUIContainer;

        private List<ParameterPropertyNodeView> ParameterNodeViews => graphElements.OfType<ParameterPropertyNodeView>().ToList();
        
        public void Dispose() {
            graphElements.OfType<IWorldGraphNodeView>().ToList().ForEach(node => node.Dispose());

            nodeCreationRequest = null;
            Object.DestroyImmediate(m_SearchWindowProvider);
            m_SearchWindowProvider = null;

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
            // ------------------ Create Base SceneHandle ------------------
            if (graph.IsEmpty) {
                graph.CreateSubAsset(typeof(BaseHandle));
            }

            // ------------------ Create nodes for every scene handle ------------------
            foreach (var sceneHandle in graph.sceneHandles) {
                CreateGraphNode(sceneHandle);
            }

            // ------------------ Create edges for the nodes ------------------
            foreach (var parent1 in graph.sceneHandles) {
                IEnumerable<SceneHandle> children = WorldGraph.GetChildren(parent1);
                foreach (var child in children) {
                    WorldGraphNodeView baseView = (WorldGraphNodeView) GetNodeByGuid(parent1.guid);
                    WorldGraphNodeView targetView = (WorldGraphNodeView) GetNodeByGuid(child.guid);
                    var edge = baseView?.output.ConnectTo(targetView?.input);
                    AddElement(edge);
                }
            }

            // ------------------ Create Parameters in Blackboard + Create Parameter Nodes ------------------
            foreach (var exposedParam in graph.allParameters) {
                exposedParametersBlackboard.Add(CreateBlackboardField(exposedParam));
                if (exposedParam.Displayed) {
                    CreateParameterGraphNode(exposedParam, exposedParam.Position);
                }
            }

            // ------------------ Connect Parameter Nodes to the respective Parameter Ports ------------------
            foreach (var sceneHandle in graph.sceneHandles) {
                WorldGraphNodeView baseView = (WorldGraphNodeView) GetNodeByGuid(sceneHandle.guid);
                List<WorldGraphPort> ports = baseView.inputContainer.Query<WorldGraphPort>().ToList();
                foreach (var parameter in sceneHandle.allParameters) {
                    ParameterPropertyNodeView paramView = (ParameterPropertyNodeView) GetNodeByGuid(parameter.GUID);
                    foreach (var port in ports) {
                        if (parameter.ConnectedPortGUID == port.PortData.GUID) {
                            var edge = paramView.output.ConnectTo(port);
                            AddElement(edge);
                        }
                    }
                }
            }
        }

        public void CreateNode(Type type, Vector2 position) {
            SceneHandle node = graph.CreateSubAsset(type);
            node.position = position;
            CreateGraphNode(node);
        }

        public void CreateGraphNode(SceneHandle sceneHandle) {
            var graphNode = new WorldGraphNodeView(this, sceneHandle, edgeConnectorListener);
            AddElement(graphNode);
        }

        public void CreateParameterGraphNode(ExposedParameter parameter, Vector2 position) {
            parameter.Position = position;
            parameter.Displayed = true;

            var outputPort = new PortData {
                PortColor = new Color(0.52f, 0.89f, 0.91f),
                PortDirection = "Output",
                PortCapacity = "Single",
                PortType = PortType.Parameter,
            };

            var outputPortView = new WorldGraphPort(outputPort, edgeConnectorListener);
            var parameterNodeView = new ParameterPropertyNodeView(parameter, outputPortView);

            AddElement(parameterNodeView);
        }

        public ExposedParameter CreateExposedParameter(ParameterType type) {
            return graph.CreateParameter(type);
        }

        public BlackboardField CreateBlackboardField(ExposedParameter parameter) {
            var field = new BlackboardField {
                userData = parameter,
                text = $"{parameter.Name}",
                typeText = parameter.ParameterType.ToString(),
                icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null
            };
            field.Bind(new SerializedObject(parameter));
            return field;
        }

        public override void AddToSelection(ISelectable selectable) {
            base.AddToSelection(selectable);
            switch (selectable) {
                case BlackboardField field:
                    DrawInspector((ExposedParameter) field.userData);
                    break;
                case ParameterPropertyNodeView paramView:
                    DrawInspector(paramView.parameter);
                    break;
            }
        }

        public void DrawInspector(Object objectToDisplay) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            switch (objectToDisplay) {
                case SceneHandle sceneHandle:
                    DrawProperties(sceneHandle, $"{sceneHandle.HandleName} Node");
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


        private void DrawProperties(Object exposedParameter, string label) {
            var serializedParameter = new SerializedObject(exposedParameter);
            var fieldInfos =
                WGReflectionHelper.GetFieldInfosWithAttribute(exposedParameter, typeof(WGInspectableAttribute));

            titleLabel.text = label;
            GUIContainer = new IMGUIContainer(() => {
                serializedParameter.Update();

                foreach (var field in fieldInfos) {
                    EditorGUILayout.PropertyField(serializedParameter.FindProperty(field.Name));
                }

                serializedParameter.ApplyModifiedProperties();
            });
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