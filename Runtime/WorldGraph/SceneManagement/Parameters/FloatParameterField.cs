using System;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class FloatParameterField : ParameterField<float> {
        public FloatParameterField() {
            Name = "FloatParameter";
            Reference = "_FloatParameter";
            Exposed = true;
            ParameterType = ParameterType.Float;
            Value = 69f;
        }
    }

}