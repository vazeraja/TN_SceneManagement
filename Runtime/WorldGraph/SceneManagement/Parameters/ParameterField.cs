using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public abstract class ParameterField<TValueType> : ExposedParameter {
        public TValueType Value;
    }

}