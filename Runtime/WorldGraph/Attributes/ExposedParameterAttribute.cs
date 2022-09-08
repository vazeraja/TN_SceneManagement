using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public enum ParameterType {
        String,
        Float,
        Int,
        Bool
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class ExposedParameterAttribute : PropertyAttribute {
        public ParameterType Type;
        public string Name;
    }

}