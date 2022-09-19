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
        public readonly WorldGraph graph;

        public WorldGraphSearcherProvider m_SearchWindowProvider;
        private EdgeConnectorListener edgeConnectorListener;

        public Blackboard inspectorBlackboard;
        public Blackboard exposedParametersBlackboard;

        private VisualElement _RootElement;
        private Label titleLabel;
        private ScrollView inspectorContentContainer;

        public SerializedObject serializedGraph;
        public WorldGraphEditor graphEditor;

        public void Dispose() {
            graphElements.OfType<IWorldGraphNodeView>().ToList().ForEach(node => node.Dispose());

            Object.DestroyImmediate(m_SearchWindowProvider);
            Object.DestroyImmediate(graphEditor);
            nodeCreationRequest = null;
            graphEditor = null;
            m_SearchWindowProvider = null;

            _RootElement = null;
            titleLabel = null;
            inspectorContentContainer = null;

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
            serializedGraph = new SerializedObject(graph);

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WorldGraphSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, this, CreateNode);
            nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != editorWindow) return;
                var displayPosition = (c.screenMousePosition - this.editorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(editorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition),
                    displayPosition, null);
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
            foreach (WorldGraphEdge graphEdge in from edge in graph.transitions
                let outputView = (WorldGraphNodeView) GetNodeByGuid(edge.OutputNodeGUID)
                let inputView = (WorldGraphNodeView) GetNodeByGuid(edge.InputNodeGUID)
                select outputView.output.ConnectTo<WorldGraphEdge>(inputView.input)) {
                AddElement(graphEdge);
            }

            // ------------------ Create Parameters ------------------
            foreach (var exposedParam in graph.allParameters) {
                CreateBlackboardField(exposedParam);
            }

            foreach (var parameterViewData in graph.ExposedParameterViewDatas) {
                var parameterView = CreateParameterGraphNode(parameterViewData);

                if (parameterViewData.connectedNode != null) {
                    WorldGraphNodeView baseView = (WorldGraphNodeView) GetNodeByGuid(parameterViewData.connectedNode.GUID);
                    List<WorldGraphPort> ports = baseView.inputContainer.Query<WorldGraphPort>().ToList();
                    var portToConnect = ports.Find(port => port.PortData.GUID == parameterViewData.connectedPortGUID);

                    WorldGraphEdge edge = ((WorldGraphPort) parameterView.output).ConnectTo<WorldGraphEdge>(portToConnect);
                    AddElement(edge);
                }
            }
            // ------------------ Connect Parameters  ------------------
            // foreach (var sceneHandle in graph.sceneHandles) {
            //     WorldGraphNodeView baseView = (WorldGraphNodeView) GetNodeByGuid(sceneHandle.GUID);
            //     List<WorldGraphPort> ports = baseView.inputContainer.Query<WorldGraphPort>().ToList();
            //     foreach (var parameter in sceneHandle.allParameters) {
            //         ExposedParameterNodeView paramView = (ExposedParameterNodeView) GetNodeByGuid(parameter.GUID);
            //         foreach (var port in ports) {
            //             if (parameter.ConnectedPortGUID == port.PortData.GUID) {
            //                 WorldGraphEdge t = ((WorldGraphPort) paramView.output).ConnectTo<WorldGraphEdge>(port);
            //                 AddElement(t);
            //             }
            //         }
            //     }
            // }
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

            UpdateSerializedProperties();
        }

        public ExposedParameterNodeView CreateParameterGraphNode(ExposedParameterViewData viewData) {
            var outputPort = new PortData {
                PortColor = new Color(0.52f, 0.89f, 0.91f),
                PortDirection = "Output",
                PortCapacity = "Single",
                PortType = PortType.Parameter,
            };
            var outputPortView = new WorldGraphPort(outputPort, edgeConnectorListener);
            var parameterNodeView = new ExposedParameterNodeView(viewData, outputPortView);

            AddElement(parameterNodeView);

            return parameterNodeView;
        }

        public void CreateParameterGraphNode(ExposedParameter parameter, Vector2 position) {
            var outputPort = new PortData {
                PortColor = new Color(0.52f, 0.89f, 0.91f),
                PortDirection = "Output",
                PortCapacity = "Single",
                PortType = PortType.Parameter,
            };
            var outputPortView = new WorldGraphPort(outputPort, edgeConnectorListener);
            InitializePortBehavior(outputPortView);

            var parameterViewData = new ExposedParameterViewData(parameter, position);
            graph.ExposedParameterViewDatas.Add(parameterViewData);

            var parameterNodeView = new ExposedParameterNodeView(parameterViewData, outputPortView);
            AddElement(parameterNodeView);
            
            UpdateSerializedProperties();
        }

        public void CreateBlackboardField(ExposedParameter parameter) {
            var blackboardField = new WGBlackboardField(parameter);
            exposedParametersBlackboard.Add(blackboardField);

            UpdateSerializedProperties();
        }

        public void UpdateBlackboardFieldName(VisualElement element, string newValue) {
            var param = (ExposedParameter) ((BlackboardField) element).userData;
            var paramNode = graphElements.OfType<ExposedParameterNodeView>().ToList().Find(view => view.userData == param);

            param.Name = newValue;
            if (paramNode != null) paramNode.output.portName = newValue;

            ((BlackboardField) element).text = newValue;
        }

        public void DrawInspector(SceneHandle sceneHandle) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();
            
            var serializedObject = new SerializedObject(sceneHandle);
            var fieldInfos = WGHelper.GetFieldInfosWithAttribute(sceneHandle, typeof(WGInspectableAttribute));
            
            titleLabel.text = $"{sceneHandle.HandleName} Node";
            IMGUIContainer GUIContainer = new IMGUIContainer(() => {
                foreach (var fieldInfo in fieldInfos) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty(fieldInfo.Name));
                }
            });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);

        }

        public void DrawInspector(ExposedParameter parameter) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            var allParamsProp = serializedGraph.FindProperty("allParameters");
            var propertyMatch = GetPropertyMatch(allParamsProp, parameter);
            
            titleLabel.text = $"{parameter.Name} Parameter";
            IMGUIContainer GUIContainer = new IMGUIContainer(() => {
                serializedGraph.Update();

                EditorGUILayout.PropertyField(propertyMatch.FindPropertyRelative("Name"));
                EditorGUILayout.PropertyField(propertyMatch.FindPropertyRelative("Reference"));
                EditorGUILayout.PropertyField(propertyMatch.FindPropertyRelative("Exposed"));
                EditorGUILayout.PropertyField(propertyMatch.FindPropertyRelative("Value"));

                serializedGraph.ApplyModifiedProperties();
            });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }
        public void DrawInspector(Transition transition) {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            var editor = UnityEditor.Editor.CreateEditor(transition);
            titleLabel.text = transition.ToString();
            IMGUIContainer GUIContainer = new IMGUIContainer(() => { editor.OnInspectorGUI(); });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }

        public void ShowGraphSettings() {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();

            titleLabel.text = "Graph Settings";
            IMGUIContainer GUIContainer = new IMGUIContainer(() => { graphEditor.OnInspectorGUI(); });

            inspectorContentContainer.Add(GUIContainer);
            inspectorBlackboard.Add(_RootElement);
        }


        public void ClearInspector() {
            inspectorBlackboard.Clear();
            inspectorContentContainer.Clear();
        }

        private void UpdateSerializedProperties() {
            serializedGraph = new SerializedObject(graph);
        }

        private static SerializedProperty GetPropertyMatch(SerializedProperty property, object referenceValue) {
            for (var i = 0; i < property.arraySize; i++) {
                var iProp = property.GetArrayElementAtIndex(i);
                if (iProp.managedReferenceValue == referenceValue) {
                    return iProp;
                }
            }

            return null;
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
                        graph.CreateTransition(output.sceneHandle, input.sceneHandle);
                        break;
                    }
                    case PortType.Parameter when node is WorldGraphNodeView nodeView: {
                        // ExposedParameter parameter = (ExposedParameter)((ExposedParameterNodeView) outputPort.node).userData;
                        // nodeView.sceneHandle.AddParameter(parameter);
                        // port.PortData.Parameter = parameter;
                        break;
                    }
                    case PortType.Parameter when node is ExposedParameterNodeView propertyNodeView:
                        var param = propertyNodeView.GetViewData();
                        var inputNodeView = (WorldGraphNodeView) inputPort.node;

                        param.connectedNode = inputNodeView.sceneHandle;
                        param.connectedPortGUID = inputPort.PortData.GUID;
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

                        var edgeToRemove = graph.transitions.Find(e =>
                            e.OutputNodeGUID == output.sceneHandle.GUID &&
                            e.InputNodeGUID == input.sceneHandle.GUID);
                        graph.RemoveTransition(edgeToRemove);

                        break;
                    }
                    case PortType.Parameter when node is WorldGraphNodeView nodeView:
                        // nodeView.sceneHandle.RemoveParameter(port.PortData.Parameter);
                        // port.PortData.Parameter = null;
                        break;
                    case PortType.Parameter when node is ExposedParameterNodeView propertyNodeView:
                        var param = propertyNodeView.GetViewData();

                        param.connectedNode = null;
                        param.connectedPortGUID = null;
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