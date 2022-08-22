using UnityEditor.Experimental.GraphView;

namespace ThunderNut.SceneManagement.Editor {
    public class BaseEdgeConnector : EdgeConnector {
        protected override void RegisterCallbacksOnTarget() {
        }

        protected override void UnregisterCallbacksFromTarget() {
        }

        public override EdgeDragHelper edgeDragHelper { get; }
    }
}