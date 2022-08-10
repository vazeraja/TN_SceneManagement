using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class WGEdge : Edge {
        public bool isConnected = false;

        public SerializableEdge serializedEdge => userData as SerializableEdge;

        readonly string edgeStyle = "Styles/WGEdge";

        protected WGGraphView owner => ((WGPort) (input ?? output)).owner.owner;
        
        public WGEdge() {
            styleSheets.Add(Resources.Load<StyleSheet>(edgeStyle));
        }
    }
}