#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    
    [CreateAssetMenu(fileName = "SceneHandle", menuName = "World Graph/Scene Handle")]
    public class BaseSceneHandle : ScriptableObject {
        
        public SceneReference scene;
        public List<string> passages = new() {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;

        private void ForceSwitchToScene() {
            SceneManager.LoadScene(scene.sceneIndex);
        }
    }
    
    [Serializable]
    public class SceneHandle {
        public SceneReference scene;
        public List<string> passages = new();
        public List<SceneConnection> sceneConnections;
    }
    
    [Serializable]
    public class SceneConnection {
        public int passage;
        public BaseSceneHandle baseSceneHandle;
        public int sceneHandlePassage;
    }
}