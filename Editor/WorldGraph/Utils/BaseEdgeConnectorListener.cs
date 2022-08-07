using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class BaseEdgeConnectorListener : IEdgeConnectorListener {
        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            
        }

        public void OnDrop(GraphView graphView, Edge edge) {
        }
    }
}