﻿using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class WGEdge : Edge {
        public WGEdge() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEdge"));
        }
    }
}