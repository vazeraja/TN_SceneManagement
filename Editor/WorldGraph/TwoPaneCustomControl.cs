using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class TwoPaneCustomControl : TwoPaneSplitView {
        public new class UxmlFactory : UxmlFactory<TwoPaneCustomControl, UxmlTraits> { }
        public new class UxmlTraits : TwoPaneSplitView.UxmlTraits {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                if (ve is not TwoPaneCustomControl ate) return;

                ate.fixedPaneInitialDimension = 950;
            }
        }
    }
}