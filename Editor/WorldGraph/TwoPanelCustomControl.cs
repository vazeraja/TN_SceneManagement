using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class TwoPanelCustomControl : TwoPaneSplitView {
        public new class UxmlFactory : UxmlFactory<TwoPanelCustomControl, TwoPaneSplitView.UxmlTraits> { }
    }
}