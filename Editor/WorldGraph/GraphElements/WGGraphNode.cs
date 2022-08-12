using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGraphNode : Node {
        public WGGraphView owner { private set; get; }
        public WGGraphNode() : base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) { }

        public void Initialize(WGGraphView owner) {
            this.owner = owner;
        }
        public virtual void OnRemoved() { }
        public virtual void OnCreated() { }
    }
}