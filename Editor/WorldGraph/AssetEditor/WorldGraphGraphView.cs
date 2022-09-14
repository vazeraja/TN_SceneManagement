using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView {
        private readonly EditorWindow window;
        private readonly WorldGraph graph;

        public Blackboard inspectorBlackboard;

        private readonly VisualElement _RootElement;

        public WorldGraphGraphView(EditorWindow window, WorldGraph graph) {
            this.window = window;
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
            _RootElement.Q<ScrollView>("content-container").Clear();

            switch (objectToDisplay) {
                case SceneHandle sceneHandle: {
                    var serializedHandle = new SerializedObject(sceneHandle);
                    var fieldInfos =
                        WGReflectionHelper.GetFieldInfosWithAttribute(sceneHandle, typeof(ShowInGraphInspectorAttribute));

                    _RootElement.Q<Label>("title-label").text = $"{sceneHandle.HandleName} Node";
                    var imguiContainer = new IMGUIContainer(() => {
                        serializedHandle.Update();

                        foreach (var field in fieldInfos) {
                            EditorGUILayout.PropertyField(serializedHandle.FindProperty(field.Name));
                        }

                        serializedHandle.ApplyModifiedProperties();
                    });
                    _RootElement.Q<ScrollView>("content-container").Add(imguiContainer);

                    inspectorBlackboard.Add(_RootElement);
                    break;
                }
                case ExposedParameter exposedParameter: {
                    switch (exposedParameter) {
                        case FloatParameterField floatParameterField:
                            DrawExposedParameterProperties(floatParameterField);
                            break;
                        case IntParameterField intParameterField:
                            DrawExposedParameterProperties(intParameterField);
                            break;
                        case BoolParameterField boolParameterField:
                            DrawExposedParameterProperties(boolParameterField);
                            break;
                        case StringParameterField stringParameterField:
                            DrawExposedParameterProperties(stringParameterField);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(exposedParameter));
                    }

                    break;
                }
            }
        }

        private void DrawExposedParameterProperties(ExposedParameter exposedParameter) {
            var serializedParameter = new SerializedObject(exposedParameter);
            var fieldInfos =
                WGReflectionHelper.GetFieldInfosWithAttribute(exposedParameter, typeof(ShowInGraphInspectorAttribute));

            _RootElement.Q<Label>("title-label").text = $"{exposedParameter.Name} Parameter";
            var imguiContainer = new IMGUIContainer(() => {
                serializedParameter.Update();

                foreach (var field in fieldInfos) {
                    EditorGUILayout.PropertyField(serializedParameter.FindProperty(field.Name));
                }

                serializedParameter.ApplyModifiedProperties();
            });
            _RootElement.Q<ScrollView>("content-container").Add(imguiContainer);

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