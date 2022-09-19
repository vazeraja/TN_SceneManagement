using System;

namespace ThunderNut.SceneManagement {

    public enum IntParamOptions {
        GreaterThan,
        LessThan,
        Equals,
        NotEquals,
    }
    [Serializable]
    public class IntParameterField : ParameterField<int> {
        public IntParamOptions options = IntParamOptions.Equals;

        public IntParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "IntParameter";
            Reference = "_IntParameter";
            Exposed = true;
            ParameterType = "Int";
            Value = 69;
        }
    }

}