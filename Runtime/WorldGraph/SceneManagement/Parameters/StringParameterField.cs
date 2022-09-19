using System;

namespace ThunderNut.SceneManagement {
    
    public enum StringParamOptions {
        Equals,
        NotEquals,
    }
    
    [Serializable]
    public class StringParameterField : ParameterField<string> {
        public StringParamOptions options = StringParamOptions.Equals;
        
        public StringParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "StringParameter";
            Reference = "_StringParameter";
            Exposed = true;
            ParameterType = "String";
            Value = "Default_Value";
        }
    }

}