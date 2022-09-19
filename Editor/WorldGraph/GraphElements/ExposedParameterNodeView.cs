using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class ExposedParameterNodeView : TokenNode, IWorldGraphNodeView {
        public Node gvNode => this;
        public SceneHandle sceneHandle => null;
        public WorldGraphGraphView graphView => GetFirstAncestorOfType<WorldGraphGraphView>();

        public ExposedParameterViewData data;
        
        public ExposedParameterNodeView(ExposedParameterViewData data, WorldGraphPort output) : base(null, output) { 
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertyNodeView"));
            this.data = data;
            userData = data.parameter;
            viewDataKey = data.parameter.GUID;

            output.portName = data.parameter.Name;
            output.portColor = output.PortData.PortColor;
            icon = data.parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null;
            style.left = data.position.x;
            style.top = data.position.y;
            
            this.Q("title-label").RemoveFromHierarchy();
            Add(new VisualElement() { name = "disabledOverlay", pickingMode = PickingMode.Ignore });
        }

        public override void OnSelected() {
            graphView.DrawInspector((ExposedParameter)userData);
            base.OnSelected();
        }

        public ExposedParameterViewData GetViewData() => data;

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            data.position.x = newPos.xMin;
            data.position.y = newPos.yMin;
        }

        public void Dispose() {
        }
        
    }

}