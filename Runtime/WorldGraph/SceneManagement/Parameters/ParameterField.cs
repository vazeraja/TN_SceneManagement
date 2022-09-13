using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public class ParameterField<TValueType> : ExposedParameter {
        public TValueType Value;
    }

}