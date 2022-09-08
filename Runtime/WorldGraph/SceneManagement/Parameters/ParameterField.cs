using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class ParameterField<TValueType> : ExposedParameter {
        [SerializeField] private TValueType m_Value;
        public TValueType Value {
            get => m_Value;
            set => m_Value = value;
        }
    }

}