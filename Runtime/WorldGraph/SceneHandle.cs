#if UNITY_EDITOR
#endif
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        public bool Active = true;
        
        public string guid;
        public string handleName = "";
        public Vector2 position;
        
        #if UNITY_EDITOR
        public virtual Color HandleColor => Color.white;
        #endif

        public SceneReference scene;
        public List<string> passages = new List<string>() {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;
    }

}