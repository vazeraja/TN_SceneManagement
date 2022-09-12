using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public sealed class WorldGraphPort : Port {
        private IEdgeConnectorListener ConnectorListener;

        public readonly PortData PortData;
        public readonly WorldGraphNodeView Node;

        public readonly Button deleteParameterButton;

        public WorldGraphPort(PortData portData, IEdgeConnectorListener connectorListener) :
            base(Orientation.Horizontal, portData.PortDirection == "Output" ? Direction.Output : Direction.Input, Capacity.Single, typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
            
            PortData = portData;
        }

        public WorldGraphPort(WorldGraphNodeView node, PortData portData, IEdgeConnectorListener connectorListener)
            : base(Orientation.Horizontal, portData.PortDirection == "Output" ? Direction.Output : Direction.Input, Capacity.Multi, typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);

            ConnectorListener = connectorListener;
            PortData = portData;
            Node = node;

            portColor = portData.PortColor;

            // Make StyleSheet for this
            if (portData.PortType == PortType.Parameter) {
                int outputPortCount = node.inputContainer.Query("connector").ToList().Count();
                portName = $"{portData.PortType.ToString()}({outputPortCount})";

                deleteParameterButton = new Button(() => { RemoveParameterPort(portData); });
                deleteParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0003");
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

        private void RemoveParameterPort(PortData portData) {
            var Edges = Node.graphView.edges.ToList();
            Edge connectedEdge = Edges.Find(edge => ((WorldGraphPort) edge.input).PortData.GUID == portData.GUID);

            if (connectedEdge != null) {
                connectedEdge.input.Disconnect(connectedEdge);
                
                portData.Parameter.ConnectedPortGUID = null;
                Node.sceneHandle.RemoveParameter(portData.Parameter);
                
                Node.graphView.RemoveElement(connectedEdge);
                Node.graphView.RemoveElement(connectedEdge.output.node);
            }
            
            Node.sceneHandle.RemovePort(portData);
            Node.inputContainer.Remove(this);

            Node.RefreshPorts();
            Node.RefreshExpandedState();
        }
    }

}