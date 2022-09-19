using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    
    [Serializable]
    public class ExposedParameterViewData {
        [SerializeReference] public ExposedParameter parameter;
         
        public SceneHandle connectedNode;
        public string connectedPortGUID;
        public Vector2 position;

        public ExposedParameterViewData(ExposedParameter parameter, Vector2 position) {
            this.parameter = parameter;
            this.position = position;
        }
    }
    
    [Serializable]
    public class ExposedParameter : ISerializationCallbackReceiver {
        public string GUID;
        [WGInspectable] public string Name;
        [WGInspectable] public string Reference;
        [WGInspectable] public bool Exposed;
        public string ParameterType;
        

        public void OnBeforeSerialize() {
            
        }

        public void OnAfterDeserialize() {
        }
    }

}