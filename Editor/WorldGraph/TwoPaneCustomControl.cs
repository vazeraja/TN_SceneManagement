using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class TwoPaneCustomControl : TwoPaneSplitView {

        public new class UxmlFactory : UxmlFactory<TwoPaneCustomControl, UxmlTraits> { }

        public new class UxmlTraits : TwoPaneSplitView.UxmlTraits {
            public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc) {
                base.Init(ve, bag, cc);
                if (!(ve is TwoPaneCustomControl ate)) return;
                
                ate.fixedPaneInitialDimension = 950;
                ate.Add(new VisualElement {name = "left-panel"});
                ate.Add(new VisualElement {name = "right-panel"});
            }
        }
    }
}