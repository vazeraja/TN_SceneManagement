using System;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class BoolParameterField : ParameterField<bool> {
        public BoolParameterField() {
            Name = "BoolParameter";
            Reference = "_BoolParameter";
            Exposed = true;
            ParameterType = ParameterType.Bool;
            Value = true;
        }
    }

}