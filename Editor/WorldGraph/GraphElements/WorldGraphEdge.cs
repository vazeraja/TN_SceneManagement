using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraphEdge : Edge {
        public WorldGraphEdge() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEdge"));
        }

        public override void OnSelected() {
            base.OnSelected();
            // Debug.Log((output.node as WorldGraphNodeView)?.sceneHandle.HandleName);
            // Debug.Log((input.node as WorldGraphNodeView)?.sceneHandle.HandleName);
        }
    }
}