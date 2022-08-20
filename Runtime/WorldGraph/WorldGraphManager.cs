using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    public class WorldGraphManager : MonoBehaviour {

        public WorldGraph worldGraph;

        private static WorldGraphManager _instance;
        public static WorldGraphManager Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = FindObjectOfType<WorldGraphManager>();
                    if(_instance == null)
                    {
                        var go = new GameObject("WorldGraphManager");
                        _instance = go.AddComponent<WorldGraphManager>();
                    }
                }
                return _instance;
            }
        }
        private void Awake()
        {
            if(Instance != this)
            {
                Debug.Log($"WorldGraphManager Duplicate: deleting {this.name}", this);
                DestroyImmediate(this.gameObject);
            } else
            {
                Debug.Log($"WorldGraphManager: {this.name}", this);
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
                #endif
                DontDestroyOnLoad(this.gameObject);
            }
        }
        private void OnDestroy()
        {
            if (_instance == this)
            {
                Debug.Log("WorldGraphManager OnDestroy: set instance to null");
                _instance = null;
            }
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
            #endif  
        }

        #if UNITY_EDITOR
        private void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange obj)
        {
            switch(obj)
            {
                case UnityEditor.PlayModeStateChange.ExitingEditMode:
                    Debug.Log("WorldGraphManager PlayModeStateChange ExitingEditMode: set instance to null");
                    _instance = null;
                    break;
            }
        }
        #endif
    }
}