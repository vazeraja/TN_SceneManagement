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
        
        private UnityEditor.Editor editor;
        private Vector2 scrollPos;

        public Blackboard inspectorBlackboard;
        
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
        }

        public override void AddToSelection(ISelectable selectable) {
            base.AddToSelection(selectable);
            if (selectable is BlackboardField field) {
                DrawInspector((ExposedParameter)field.userData);
            }
        }

        public void DrawInspector(Object objectToDisplay) {
            inspectorBlackboard.Clear();
            Object.DestroyImmediate(editor);

            editor = UnityEditor.Editor.CreateEditor(objectToDisplay);

            var section = new BlackboardSection{title = $"{objectToDisplay.name} Node"};
            section.Q<Label>("sectionTitleLabel").style.color = new Color(0.77f, 0.77f, 0.77f);
            section.Q<Label>("sectionTitleLabel").style.unityFontStyleAndWeight = FontStyle.Bold;
            section.Q<Label>("sectionTitleLabel").style.fontSize = 15;

            inspectorBlackboard.Add(section);
            inspectorBlackboard.Add(new IMGUIContainer(() => {
                using var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos);
                scrollPos = scrollViewScope.scrollPosition;
                if (editor && editor.target) {
                    editor.OnInspectorGUI();
                }
            }));
        }

        public void RegisterPortCallbacks() {
            ports.OfType<WorldGraphPort>().ToList().ForEach(InitializePortBehavior);
        }

        public void InitializePortBehavior(WorldGraphPort worldGraphPort) {
            // ------------ Dragging an edge disconnects both ports ------------
            worldGraphPort.OnConnected += (node, port, edge) => {
                WorldGraphPort outputPort = (WorldGraphPort) edge.output;
                WorldGraphPort inputPort = (WorldGraphPort) edge.input;

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
            return field;
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