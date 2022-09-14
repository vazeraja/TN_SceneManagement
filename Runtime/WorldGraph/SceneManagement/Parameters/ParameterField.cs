using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public class ParameterField<TValueType> : ExposedParameter {
        [WGInspectable(ChangeCheck = true)]
        public TValueType Value;
    }

}