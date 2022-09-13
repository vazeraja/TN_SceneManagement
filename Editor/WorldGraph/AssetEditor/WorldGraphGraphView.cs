using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView {
        private WorldGraph graph;

        public WorldGraphGraphView(WorldGraph graph) {
            this.graph = graph;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());

            SetupZoom(0.05f, 8);
            AddToClassList("drop-area");
        }

        public void RegisterPortCallbacks() {
            var graphPorts = ports.OfType<WorldGraphPort>().ToList();
            foreach (var worldGraphPort in graphPorts) {
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
                            ExposedParameter parameter = ((ParameterPropertyNodeView) outputPort.node).parameter;
                            nodeView.sceneHandle.AddParameter(parameter);
                            port.PortData.Parameter = parameter;
                            break;
                        }
                        case PortType.Parameter when node is ParameterPropertyNodeView paramNodeView:
                            paramNodeView.parameter.ConnectedPortGUID = inputPort.PortData.GUID;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
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
                            nodeView.sceneHandle.RemoveParameter(port.PortData.Parameter);
                            port.PortData.Parameter = null;
                            break;
                        case PortType.Parameter when node is ParameterPropertyNodeView propertyNodeView:
                            propertyNodeView.parameter.ConnectedPortGUID = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                };
            }
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    ((WorldGraphPort) endPort).PortData.OwnerNodeGUID != ((WorldGraphPort) startPort).PortData.OwnerNodeGUID &&
                    ((WorldGraphPort) endPort).PortData.PortDirection != ((WorldGraphPort) startPort).PortData.PortDirection &&
                    ((WorldGraphPort) endPort).PortData.PortType == ((WorldGraphPort) startPort).PortData.PortType)
                .ToList();
        }

        public override void AddToSelection(ISelectable selectable) {
            base.AddToSelection(selectable);
        }

        public void RegisterCompleteObjectUndo(string name) {
            Undo.RegisterCompleteObjectUndo(graph, name);
        }
    }

}