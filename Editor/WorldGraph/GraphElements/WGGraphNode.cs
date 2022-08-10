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
        public SceneHandle nodeTarget;

        public List<WGPort> inputPortViews = new List<WGPort>();
        public List<WGPort> outputPortViews = new List<WGPort>();
        private Dictionary<string, List<WGPort>> portsPerFieldName = new Dictionary<string, List<WGPort>>();

        public const string nodeUXMLPath = "UXML/WGGraphNode";

        public WGGraphNode() : base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>(nodeUXMLPath))) { }

        public void Initialize(WGGraphView owner, SceneHandle node) {
            nodeTarget = node;
            this.owner = owner;
        }

        public List<WGPort> GetPortViewsFromFieldName(string fieldName) {
            List<WGPort> ret;

            portsPerFieldName.TryGetValue(fieldName, out ret);

            return ret;
        }

        public WGPort GetFirstPortViewFromFieldName(string fieldName) {
            return GetPortViewsFromFieldName(fieldName)?.First();
        }

        public WGPort GetPortViewFromFieldName(string fieldName, string identifier) {
            return GetPortViewsFromFieldName(fieldName)?.FirstOrDefault(pv => {
                return (pv.portData.identifier == identifier) ||
                       (String.IsNullOrEmpty(pv.portData.identifier) && String.IsNullOrEmpty(identifier));
            });
        }

        public virtual void OnRemoved() { }
        public virtual void OnCreated() { }
    }
}