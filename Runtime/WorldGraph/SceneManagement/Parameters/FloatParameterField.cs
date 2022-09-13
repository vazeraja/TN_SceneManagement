using System;

namespace ThunderNut.SceneManagement {
    
    public class FloatParameterField : ParameterField<float> {
        public FloatParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "FloatParameter";
            Reference = "_FloatParameter";
            Exposed = true;
            ParameterType = ParameterType.Float;
            Value = 69f;
        }
    }

}