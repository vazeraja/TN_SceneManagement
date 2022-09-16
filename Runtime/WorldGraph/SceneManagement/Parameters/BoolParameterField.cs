using System;

namespace ThunderNut.SceneManagement {

    public class BoolParameterField : ParameterField<bool> {
        public BoolParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "BoolParameter";
            Reference = "_BoolParameter";
            Exposed = true;
            ParameterType = ParameterType.Bool;
            Value = true;
        }
        
    }

}