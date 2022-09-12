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
        public string OwnerNodeGUID;
        public string GUID;
        
        public string PortDirection;
        public PortType PortType;
        public Color PortColor;

        public ExposedParameter Parameter;
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
        
        public List<StringParameterField> stringParameters = new List<StringParameterField>();
        public List<FloatParameterField> floatParameters = new List<FloatParameterField>();
        public List<IntParameterField> intParameters = new List<IntParameterField>();
        public List<BoolParameterField> boolParameters = new List<BoolParameterField>();

        public abstract void ChangeToScene();

        public void AddParameter(ExposedParameter param) {
            switch (param.ParameterType) {
                case ParameterType.String:
                    stringParameters.Add(param as StringParameterField);
                    break;
                case ParameterType.Float:
                    floatParameters.Add(param as FloatParameterField);
                    break;
                case ParameterType.Int:
                    intParameters.Add(param as IntParameterField);
                    break;
                case ParameterType.Bool:
                    boolParameters.Add(param as BoolParameterField);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public void RemoveParameter(ExposedParameter param) {
            switch (param.ParameterType) {
                case ParameterType.String:
                    stringParameters.Remove(param as StringParameterField);
                    break;
                case ParameterType.Float:
                    floatParameters.Remove(param as FloatParameterField);
                    break;
                case ParameterType.Int:
                    intParameters.Remove(param as IntParameterField);
                    break;
                case ParameterType.Bool:
                    boolParameters.Remove(param as BoolParameterField);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = GUID.Generate().ToString(),
                
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

    #endif

}