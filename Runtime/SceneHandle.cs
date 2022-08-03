#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    
    [Serializable]
    public class PassageElement {
        public int sceneTag;
        public SceneHandle sceneHandle;
        public int sceneHandleTags;
    }

    [CreateAssetMenu(fileName = "SceneHandle", menuName = "World Graph/Scene Handle")]
    public class SceneHandle : ScriptableObject {
        public SceneReference scene;
        
        public string[] sceneTags = {"default_value1", "default_value2"};
        public List<PassageElement> passageElements;

        private void OnEnable() {
            
        }
    }
}