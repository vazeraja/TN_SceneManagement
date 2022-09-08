using System;
using UnityEngine;
using Object = System.Object;

namespace ThunderNut.SceneManagement {

    public enum ParameterType {
        String,
        Float,
        Int,
        Bool
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ParameterAttribute : PropertyAttribute {
        public ParameterType Type;
        public string Name;
    }

}