using System;
using UnityEngine;
using Object = System.Object;

namespace ThunderNut.SceneManagement {

    public enum ParameterType {
        StringParam,
        FloatParam,
        IntParam,
        BoolParam
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ParameterFilterAttribute : PropertyAttribute {
        public ParameterType Type;
        public string Name;
    }

}