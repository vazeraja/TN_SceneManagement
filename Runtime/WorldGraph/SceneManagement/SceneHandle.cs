using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        public bool Active = true;
        public string HandleName = "";

        public string guid;
        public Vector2 position;

        #if UNITY_EDITOR
        public virtual Color HandleColor => Color.white;
        #endif

        public SceneReference scene;
        public List<string> passages = new List<string> {"default_value1", "default_value2"};
        public SceneConnectionsList sceneConnections;

        protected static void LoadSceneFromConnection(SceneConnection connection) {
            SceneManager.LoadScene(connection.exitScene.scene.ScenePath);
        }

        public abstract void ChangeToScene();
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : Editor {
        private bool _settingsDropdown;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            const bool disabled = true;
            using (new EditorGUI.DisabledGroupScope(disabled)) {
                _settingsDropdown = EditorGUILayout.Foldout(_settingsDropdown, "Internal Settings", true, EditorStyles.foldout);
                if (_settingsDropdown) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandleName"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("passages"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneConnections"));

            serializedObject.ApplyModifiedProperties();
        }
    }

    #endif

}