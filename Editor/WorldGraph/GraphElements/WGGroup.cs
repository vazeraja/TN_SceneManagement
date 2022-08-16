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

        private Label titleLabel;
        private ColorField colorField;

        readonly string groupStyle = "Styles/WGGroup";

        public WGGroup() {
            styleSheets.Add(Resources.Load<StyleSheet>(groupStyle));
        }
        
    }
}