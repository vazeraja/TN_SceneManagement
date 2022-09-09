using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class PortData {
        public bool IsOutputPort;
        public string PortDirection;
        public PortType PortType;
        public Color PortColor;
    }

    public enum PortType {
        Default,
        Parameter,
    }
    public abstract class SceneHandle : ScriptableObject {
        public string guid;
        public Vector2 position;
        public List<PortData> ports = new List<PortData>();

        public bool Active = true;
        public string HandleName = "";
        protected virtual Color HandleColor => Color.white;
        public Color color => HandleColor;

        public SceneReference scene;
        public List<SceneHandle> children = new List<SceneHandle>();

        public abstract void ChangeToScene();

        public PortData CreatePort(bool isOutput, bool isParameter, Color portColor) {
            var portData = new PortData {
                IsOutputPort = isOutput,
                PortDirection = isOutput ? "Output" : "Input",
                PortType = isParameter ? PortType.Parameter : PortType.Default,
                PortColor = portColor,
            };
            ports.Add(portData);
            return portData;
        }

        public void RemovePort(PortData portData) {
            ports.Remove(portData);
        }
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

            EditorGUILayout.PropertyField(serializedObject.FindProperty("ports"));
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