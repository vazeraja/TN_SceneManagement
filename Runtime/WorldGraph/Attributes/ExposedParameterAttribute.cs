using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {


    [AttributeUsage(AttributeTargets.Field)]
    public class ExposedParameterAttribute : PropertyAttribute {
        public string Name;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class WGInspectableAttribute : PropertyAttribute {
        public bool ChangeCheck;
    }

}