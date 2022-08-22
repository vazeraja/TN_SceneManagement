#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        [Tooltip("Whether or not this handle is active")]
        public bool Active = true;

        [Tooltip("Unique identifier for this handle")]
        public string guid;

        #if UNITY_EDITOR
        public virtual Color HandleColor => Color.white;
        #endif
        public virtual string HandleName => "";


        public SceneReference scene;
        public List<string> passages = new() {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;
    }

    [Serializable]
    public class SceneConnection {
        public int passage;
        public SceneHandle sceneHandle;
        public int sceneHandlePassage;
    }

}