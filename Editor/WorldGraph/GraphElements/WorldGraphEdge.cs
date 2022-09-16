using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEdge : Edge {
        public WorldGraphGraphView graphView => GetFirstAncestorOfType<WorldGraphGraphView>();

        public WorldGraphEdge() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEdge"));
        }

        public override void OnSelected() {
            if (output.node is not ParameterPropertyNodeView) {
                graphView.ShowTransitionInformation(
                    ((WorldGraphNodeView) output.node).sceneHandle,
                        ((WorldGraphNodeView) input.node).sceneHandle
                );
            }
            base.OnSelected();
        }
    }

}