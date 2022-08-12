using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGroup : UnityEditor.Experimental.GraphView.Group {
        public WGGraphView owner;
        public Group group;

        private Label titleLabel;
        private ColorField colorField;

        readonly string groupStyle = "Styles/WGGroup";

        public WGGroup() {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
        }

        private static void BuildContextualMenu(ContextualMenuPopulateEvent evt) { }
        
        protected override void OnElementsAdded(IEnumerable<GraphElement> elements) {
            base.OnElementsAdded(elements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements) {
            base.OnElementsRemoved(elements);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            group.position = newPos;
        }
    }
}