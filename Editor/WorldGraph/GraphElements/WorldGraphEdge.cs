using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraphEdge : Edge {
        public WorldGraphEdge() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEdge"));
        }
    }
}