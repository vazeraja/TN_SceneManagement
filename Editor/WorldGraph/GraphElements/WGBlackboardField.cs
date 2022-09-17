using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class WGBlackboardField : BlackboardField {
        private WorldGraphGraphView graphView => GetFirstAncestorOfType<WorldGraphGraphView>();

        public WGBlackboardField(ExposedParameter parameter) {
            userData = parameter;
            text = $"{parameter.Name}";
            typeText = parameter.ParameterType;
            icon = parameter.Exposed ? Resources.Load<Texture2D>("GraphView/Nodes/BlackboardFieldExposed") : null;
        }
        public override void OnSelected() {
            graphView.DrawInspector((ExposedParameter) userData);
            base.OnSelected();
        }
    }

}