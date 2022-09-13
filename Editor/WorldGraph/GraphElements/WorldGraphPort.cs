using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public sealed class WorldGraphPort : Port {
        public readonly PortData PortData;

        public readonly Button deleteParameterButton;

        public event Action<Node, WorldGraphPort, Edge> OnConnected;
        public event Action<Node, WorldGraphPort, Edge> OnDisconnected;

        public WorldGraphPort(PortData portData, IEdgeConnectorListener connectorListener)
            : base(Orientation.Horizontal, portData.PortDirection == "Output" ? Direction.Output : Direction.Input, Capacity.Single,
                typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
            
            PortData = portData;
            portColor = portData.PortColor;
        }

        public WorldGraphPort(Node node, PortData portData, IEdgeConnectorListener connectorListener)
            : base(Orientation.Horizontal, portData.PortDirection == "Output" ? Direction.Output : Direction.Input, Capacity.Multi,
                typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
            
            PortData = portData;
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

        public override void Connect(Edge edge) {
            OnConnected?.Invoke(node, this, edge);
            base.Connect(edge);
        }
        public override void Disconnect(Edge edge) {
            OnDisconnected?.Invoke(node, this, edge);
            base.Disconnect(edge);
        }

        public void RemoveParameterPort(PortData portData) {
            var Edges = ((WorldGraphNodeView) node).graphView.edges.ToList();

            Edge connectedEdge = Edges.Find(edge => ((WorldGraphPort) edge.input).PortData.GUID == portData.GUID);

            if (connectedEdge != null) {
                connectedEdge.input.Disconnect(connectedEdge);
                connectedEdge.output.Disconnect(connectedEdge);

                m_GraphView.RemoveElement(connectedEdge);
            }

            ((WorldGraphNodeView) node).sceneHandle.RemovePort(portData);
            node.inputContainer.Remove(this);
        }
    }

}