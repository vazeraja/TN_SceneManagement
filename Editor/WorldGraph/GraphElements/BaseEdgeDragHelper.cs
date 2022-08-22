using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class BaseEdgeDragHelper : EdgeDragHelper {
        public override bool HandleMouseDown(MouseDownEvent evt) {
            throw new System.NotImplementedException();
        }

        public override void HandleMouseMove(MouseMoveEvent evt) {
        }

        public override void HandleMouseUp(MouseUpEvent evt) {
        }

        public override void Reset(bool didConnect = false) {
        }

        public override Edge edgeCandidate { get; set; }
        public override Port draggedPort { get; set; }
    }
}