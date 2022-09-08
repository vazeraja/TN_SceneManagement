using System;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class IntParameterField : ParameterField<int> {
        public IntParameterField() {
            Name = "IntParameter";
            Reference = "_IntParameter";
            Exposed = true;
            ParameterType = ParameterType.Int;
            Value = 69;
        }
    }

}