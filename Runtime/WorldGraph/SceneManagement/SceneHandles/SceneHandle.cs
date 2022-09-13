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

        [SerializeField, SerializeReference]
        private List<StringParameterField> stringParameters = new List<StringParameterField>();
        [SerializeField, SerializeReference]
        private List<FloatParameterField> floatParameters = new List<FloatParameterField>();
        [SerializeField, SerializeReference]
        private List<IntParameterField> intParameters = new List<IntParameterField>();
        [SerializeField, SerializeReference]
        private List<BoolParameterField> boolParameters = new List<BoolParameterField>();

        public IEnumerable<ExposedParameter> allParameters {
            get {
                List<ExposedParameter> list = new List<ExposedParameter>();
                list.AddRange(stringParameters);
                list.AddRange(floatParameters);
                list.AddRange(intParameters);
                list.AddRange(boolParameters);
                return list;
            }
        }

        public abstract void ChangeToScene();

        #if UNITY_EDITOR

        public void AddParameter(ExposedParameter param) {
            switch (param.ParameterType) {
                case ParameterType.String:
                    stringParameters.Add((StringParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Float:
                    floatParameters.Add((FloatParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Int:
                    intParameters.Add((IntParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Bool:
                    boolParameters.Add((BoolParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveParameter(ExposedParameter param) { 
            switch (param.ParameterType) {
                case ParameterType.String:
                    stringParameters.Remove((StringParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Float:
                    floatParameters.Remove((FloatParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Int:
                    intParameters.Remove((IntParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                case ParameterType.Bool:
                    boolParameters.Remove((BoolParameterField) param);
                    EditorUtility.SetDirty(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortType = isParameter ? PortType.Parameter : PortType.Default,
                PortColor = portColor,
            };
            ports.Add(portData);
            EditorUtility.SetDirty(this);
            return portData;
        }

        public void RemovePort(PortData portData) {
            ports.Remove(portData);
            EditorUtility.SetDirty(this);
        }
        #endif
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