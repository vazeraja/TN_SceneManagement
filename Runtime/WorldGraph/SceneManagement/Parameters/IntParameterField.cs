using System;

namespace ThunderNut.SceneManagement {
    
    public class IntParameterField : ParameterField<int> {
        public IntParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "IntParameter";
            Reference = "_IntParameter";
            Exposed = true;
            ParameterType = ParameterType.Int;
            Value = 69;
        }

    }

}