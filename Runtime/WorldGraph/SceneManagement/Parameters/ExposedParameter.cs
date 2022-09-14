using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public class ExposedParameter : ScriptableObject {
        [ShowInGraphInspector]
        public string Name;
        [ShowInGraphInspector]
        public string Reference;
        [ShowInGraphInspector]
        public bool Exposed;

        public ParameterType ParameterType;
        public string GUID;
        public Vector2 Position;
        public string ConnectedPortGUID;
        public bool Displayed;

        public void Awake() {
            name = Name;
        }
    }

}