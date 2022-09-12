using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class ParameterPropertyNodeView : TokenNode {
        private readonly BlackboardField blackboardField;
        public readonly ExposedParameter Parameter;
        
        public ParameterPropertyNodeView(BlackboardField blackboardField, Port output) : base(null, output) {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PropertyNodeView"));
            this.blackboardField = blackboardField;
            Parameter = (ExposedParameter) blackboardField.userData;
            
            output.portName = blackboardField.text;
            output.portColor = ((WorldGraphPort) output).PortData.PortColor;
            icon = blackboardField.icon;
            style.left = Parameter.position.x;
            style.top = Parameter.position.y;

            Parameter.Displayed = true;
            
            this.Q("title-label").RemoveFromHierarchy();
            Add(new VisualElement() { name = "disabledOverlay", pickingMode = PickingMode.Ignore });
        }
        
        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            Parameter.position.x = newPos.xMin;
            Parameter.position.y = newPos.yMin;
        }
    }

}