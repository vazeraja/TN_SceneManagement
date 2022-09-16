using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public sealed class WorldGraphPort : Port {
        public readonly PortData PortData;

        public Button deleteParameterButton;

        public Node nodeView;

        public event Action<Node, WorldGraphPort, Edge> OnConnected;
        public event Action<Node, WorldGraphPort, Edge> OnDisconnected;
        
        // public WorldGraphEdge Connect(Port other) => ConnectTo<WorldGraphEdge>(other);

        public WorldGraphPort(PortData portData, IEdgeConnectorListener connectorListener, Node nodeView = null) :
            base(Orientation.Horizontal, portData.PortDirection == "Output" ? Direction.Output : Direction.Input,
                portData.PortCapacity == "Multi" ? Capacity.Multi : Capacity.Single, typeof(bool)) {
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);

            PortData = portData;
            portColor = portData.PortColor;
            portName = portData.PortDirection;
            this.nodeView = nodeView;

            CheckPortType(portData, nodeView);
        }

        private void CheckPortType(PortData portData, Node nodeView) {
            if (portData.PortType == PortType.Parameter && nodeView != null) {
                int outputPortCount = nodeView.inputContainer.Query("connector").ToList().Count;
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
            var Edges = ((WorldGraphNodeView) nodeView).graphView.edges.ToList();

            Edge connectedEdge = Edges.Find(edge => ((WorldGraphPort) edge.input).PortData.GUID == portData.GUID);

            if (connectedEdge != null) {
                connectedEdge.input.Disconnect(connectedEdge);
                connectedEdge.output.Disconnect(connectedEdge);

                m_GraphView.RemoveElement(connectedEdge);
            }

            ((WorldGraphNodeView) nodeView).sceneHandle.RemovePort(portData);
            nodeView.inputContainer.Remove(this);
        }
    }

}