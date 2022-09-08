using System;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class StringParameterField : ParameterField<string> {
        public StringParameterField() {
            Name = "StringParameter";
            Reference = "_StringParameter";
            Exposed = true;
            ParameterType = ParameterType.String;
            Value = "Default_Value";
        }
    }

}