using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        public string guid;
        public Vector2 position;

        public bool Active = true;
        public string HandleName = "";
        protected virtual Color HandleColor => Color.white;
        public Color color => HandleColor;

        public SceneReference scene;
        public List<SceneHandle> children = new List<SceneHandle>();
        
        public List<string> passages = new List<string> {"default_value1", "default_value2"};

        public abstract void ChangeToScene();
    }

    #if UNITY_EDITOR

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : Editor {
        private bool _settingsDropdown;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true)) {
                _settingsDropdown = EditorGUILayout.Foldout(_settingsDropdown, "Internal Settings", true, EditorStyles.foldout);
                if (_settingsDropdown) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("position"));
                }
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandleName"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("children"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("passages"));
            // EditorGUILayout.PropertyField(serializedObject.FindProperty("sceneConnections"));

            serializedObject.ApplyModifiedProperties();
        }
    }

    #endif

}