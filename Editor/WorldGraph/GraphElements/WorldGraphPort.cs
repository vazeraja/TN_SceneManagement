using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphPort : Port {
        private IEdgeConnectorListener connectorListener;

        public WorldGraphPort(
            Orientation portOrientation,
            Direction portDirection,
            Capacity portCapacity,
            Type type,
            IEdgeConnectorListener connectorListener) :
            base(portOrientation, portDirection, portCapacity, type) {
            this.connectorListener = connectorListener;
            m_EdgeConnector = new EdgeConnector<WorldGraphEdge>(connectorListener);
            this.AddManipulator(m_EdgeConnector);
        }
    }

}