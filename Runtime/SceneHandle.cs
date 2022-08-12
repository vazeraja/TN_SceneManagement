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
    [Serializable]
    public class SceneConnection {
        public int passage;
        public SceneHandle sceneHandle;
        public int sceneHandlePassage;
    }

    public abstract class SceneHandle : ScriptableObject {
        #region Runtime Code

        public SceneReference scene;
        public List<string> passages = new List<string> {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;

        protected virtual void ForceSwitchToScene() {
            SceneManager.LoadScene(scene.sceneIndex);
        }

        #endregion
    }
}