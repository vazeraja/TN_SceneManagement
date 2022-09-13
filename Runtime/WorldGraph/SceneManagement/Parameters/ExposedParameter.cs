using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    
    public class ExposedParameter : ScriptableObject {
        public string GUID;
        public string Name;
        public string Reference;
        public bool Exposed;
        public bool Displayed;
        public ParameterType ParameterType;
        
        public string ConnectedPortGUID;
        public Vector2 Position;

        public void Awake() {
            name = Name;
        } 
    }
 
}