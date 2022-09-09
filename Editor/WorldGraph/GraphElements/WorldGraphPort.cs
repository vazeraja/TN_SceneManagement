using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public sealed class WorldGraphPort : Port {
        private IEdgeConnectorListener ConnectorListener;

        public readonly PortData PortData;
        public readonly WorldGraphNodeView Node;

        public readonly Button deleteParameterButton;

        public WorldGraphPort(WorldGraphNodeView node, PortData portData, IEdgeConnectorListener connectorListener)
            : base(Orientation.Horizontal, portData.IsOutputPort ? Direction.Output : Direction.Input, Capacity.Multi, typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);

            ConnectorListener = connectorListener;
            PortData = portData;
            Node = node;

            portColor = portData.PortColor;

            // Make StyleSheet for this
            if (portData.PortType == PortType.Parameter) {
                portName = "Parameter";

                deleteParameterButton = new Button(() => { RemoveParameterPort(this); }) {text = "X"};
                deleteParameterButton.style.width = 15;
                deleteParameterButton.style.height = 15;
                deleteParameterButton.style.marginLeft = 3;
                deleteParameterButton.style.marginRight = 3;
                deleteParameterButton.style.marginTop = 6;
                deleteParameterButton.style.marginBottom = 5;

                contentContainer.Add(deleteParameterButton);
            }
            else {
                portName = portData.PortDirection;
            }
        }

        private void RemoveParameterPort(WorldGraphPort port) {
            var targetEdge = Node.graphView.edges
                .Where(x => x.output.portName == port.portName && x.output.node == port.Node)
                .ToList();
            if (targetEdge.Any()) {
                var edge = targetEdge.First();
                edge.input.Disconnect(edge);
                Node.graphView.RemoveElement(targetEdge.First());
            }

            Node.sceneHandle.RemovePort(port.PortData);
            Node.inputContainer.Remove(port);

            Node.RefreshPorts();
            Node.RefreshExpandedState();
        }
    }

}