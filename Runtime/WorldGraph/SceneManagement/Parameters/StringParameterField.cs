using System;

namespace ThunderNut.SceneManagement {
    
    public class StringParameterField : ParameterField<string> {
        public StringParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "StringParameter";
            Reference = "_StringParameter";
            Exposed = true;
            ParameterType = ParameterType.String;
            Value = "Default_Value";
        }
    }

}