#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    [Serializable]
    public class SceneConnection {
        public int passage;
        public SceneHandle sceneHandle;
        public int sceneHandlePassage;
    }

    [CreateAssetMenu(fileName = "SceneHandle", menuName = "World Graph/Scene Handle")]
    public class SceneHandle : ScriptableObject {
        public SceneReference scene;

        public List<string> passages = new List<string> {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;

        private void OnEnable() { }
    }
}