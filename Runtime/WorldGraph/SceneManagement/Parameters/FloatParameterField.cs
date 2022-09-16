using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public enum FloatParamOptions {
        GreaterThan = 0,
        LessThan = 1,
    }

    public class FloatParameterField : ParameterField<float> {
        public FloatParamOptions options = FloatParamOptions.GreaterThan;

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