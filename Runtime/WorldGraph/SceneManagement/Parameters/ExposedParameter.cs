using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    
    public abstract class ExposedParameter : ScriptableObject {
        [WGInspectable]
        public string Name;
        [WGInspectable]
        public string Reference;
        [WGInspectable]
        public bool Exposed;

        public ParameterType ParameterType;
        public string GUID;
        public Vector2 Position;
        public string ConnectedPortGUID;
        public bool Displayed;

        private void Awake() {
            name = Name;
        }
    }


}