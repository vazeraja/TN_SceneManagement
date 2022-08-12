using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGraphView : GraphView {
        private readonly WorldGraph graph;

        public WGGraphView() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
        }
        public WGGraphView(WorldGraph graph) : this() {
            this.graph = graph;
        }
    }
}