using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public enum FloatParamOptions {
        GreaterThan,
        LessThan
    }

    public class FloatParameterField : ParameterField<float> {
        public FloatParamOptions options = FloatParamOptions.GreaterThan;

        public FloatParameterField() {
            GUID = Guid.NewGuid().ToString();
            Name = "FloatParameter";
            Reference = "_FloatParameter";
            Exposed = true;
            ParameterType = "Float";
            Value = 69f;
        }
    }

}