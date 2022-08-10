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

        public void Initialize(WGGraphView graphView, Group block) {
            group = block;
            owner = graphView;

            title = block.title;
            SetPosition(block.position);

            this.AddManipulator(new ContextualMenuManipulator(BuildContextualMenu));

            headerContainer.Q<TextField>().RegisterCallback<ChangeEvent<string>>(TitleChangedCallback);
            titleLabel = headerContainer.Q<Label>();

            colorField = new ColorField {value = group.color, name = "headerColorPicker"};
            colorField.RegisterValueChangedCallback(e => { UpdateGroupColor(e.newValue); });
            UpdateGroupColor(group.color);

            headerContainer.Add(colorField);

            InitializeInnerNodes();
        }

        void InitializeInnerNodes() {
            foreach (string nodeGUID in group.innerNodeGUIDs.ToList()) {
                if (!owner.graph.nodesPerGUID.ContainsKey(nodeGUID)) {
                    Debug.LogWarning("Node GUID not found: " + nodeGUID);
                    group.innerNodeGUIDs.Remove(nodeGUID);
                    continue;
                }

                var node = owner.graph.nodesPerGUID[nodeGUID];
                var nodeView = owner.nodeViewsPerNode[node];

                AddElement(nodeView);
            }
        }

        protected override void OnElementsAdded(IEnumerable<GraphElement> elements) {
            var graphElements = elements.ToList();
            foreach (var element in graphElements) {
                var node = element as WGGraphNode;

                // Adding an element that is not a node currently supported
                if (node == null)
                    continue;

                if (!group.innerNodeGUIDs.Contains(node.nodeTarget.GUID))
                    group.innerNodeGUIDs.Add(node.nodeTarget.GUID);
            }

            base.OnElementsAdded(graphElements);
        }

        protected override void OnElementsRemoved(IEnumerable<GraphElement> elements) {
            // Only remove the nodes when the group exists in the hierarchy
            var graphElements = elements.ToList();
            if (parent != null) {
                foreach (var elem in graphElements) {
                    if (elem is WGGraphNode nodeView) {
                        group.innerNodeGUIDs.Remove(nodeView.nodeTarget.GUID);
                    }
                }
            }

            base.OnElementsRemoved(graphElements);
        }

        public void UpdateGroupColor(Color newColor) {
            group.color = newColor;
            style.backgroundColor = newColor;
        }

        void TitleChangedCallback(ChangeEvent<string> e) {
            group.title = e.newValue;
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            group.position = newPos;
        }
    }
}