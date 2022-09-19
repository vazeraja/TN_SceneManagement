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
            if (output.node is not ExposedParameterNodeView) {
                var outputView = (WorldGraphNodeView) output.node;
                var inputView = (WorldGraphNodeView) input.node;
                var transition = graphView.graph.transitions.Find(e =>
                    e.OutputNodeGUID == outputView.sceneHandle.GUID && e.InputNodeGUID == inputView.sceneHandle.GUID);

                graphView.DrawInspector(transition);
            }

            base.OnSelected();
        }
    }

}