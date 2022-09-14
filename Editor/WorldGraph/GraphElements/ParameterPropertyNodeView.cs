using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class ParameterPropertyNodeView : TokenNode, IWorldGraphNodeView {
        public readonly ExposedParameter parameter;

        public Node gvNode => this;
        public SceneHandle sceneHandle => null;
        public WorldGraphGraphView graphView => GetFirstAncestorOfType<WorldGraphGraphView>();
        
        public ParameterPropertyNodeView(ExposedParameter parameter, Port output) : base(null, output) { 
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertyNodeView"));
            viewDataKey = parameter.GUID;
            this.parameter = parameter;
            
            output.portName = parameter.Name;
            output.portColor = ((WorldGraphPort) output).PortData.PortColor;
            icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null;
            style.left = this.parameter.Position.x;
            style.top = this.parameter.Position.y;
            
            this.Q("title-label").RemoveFromHierarchy();
            Add(new VisualElement() { name = "disabledOverlay", pickingMode = PickingMode.Ignore });
        }
        
        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            parameter.Position.x = newPos.xMin;
            parameter.Position.y = newPos.yMin;
        }

        public void Dispose() {
        }
        
    }

}