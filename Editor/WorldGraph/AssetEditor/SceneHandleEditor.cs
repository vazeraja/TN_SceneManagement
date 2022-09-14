using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        private bool _settingsDropdown;
        
        public override void OnInspectorGUI() {
            DrawProperties();
        }

        private void DrawProperties() {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true)) {
                _settingsDropdown = EditorGUILayout.Foldout(_settingsDropdown, "Internal Settings", true, EditorStyles.foldout);
                if (_settingsDropdown) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("position"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("ports"));
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandleName"));

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("children"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floatParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boolParameters"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}