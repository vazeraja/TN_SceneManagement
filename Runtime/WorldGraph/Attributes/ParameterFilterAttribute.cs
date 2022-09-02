using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public enum ParameterType {
        StringParam,
        FloatParam,
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ParameterFilterAttribute : PropertyAttribute {
        public ParameterType Type;
        public string Name;
    }

}