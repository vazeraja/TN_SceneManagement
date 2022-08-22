using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class BaseEdgeConnectorListener : IEdgeConnectorListener {
        readonly WorldGraph m_Graph;
        readonly WGSearcherProvider m_SearchWindowProvider;
        readonly EditorWindow m_editorWindow;
        
        public BaseEdgeConnectorListener(WorldGraph graph, WGSearcherProvider searchWindowProvider, EditorWindow editorWindow)
        {
            m_Graph = graph;
            m_SearchWindowProvider = searchWindowProvider;
            m_editorWindow = editorWindow;
        }
        
        public void OnDropOutsidePort(Edge edge, Vector2 position) {
            
        }

        public void OnDrop(GraphView graphView, Edge edge) {
        }
    }
}