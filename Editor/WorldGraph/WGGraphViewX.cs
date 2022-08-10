using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGraphViewX : GraphView {
        private readonly WorldGraph graph;

        public WGGraphViewX() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphViewX"));
        }
        public WGGraphViewX(WorldGraph graph) : this() {
            this.graph = graph;
        }
    }
}