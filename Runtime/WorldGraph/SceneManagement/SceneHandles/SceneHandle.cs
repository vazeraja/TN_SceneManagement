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
        public string PortCapacity;
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
        protected virtual Color HandleColor => Color.white;
        public virtual Color color => HandleColor;

        [WGInspectable]
        public bool Active = true;
        [WGInspectable]
        public string HandleName = "";
        [WGInspectable]
        public SceneReference scene;
        [WGInspectable]
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
            switch (param) {
                case StringParameterField stringParameterField:
                    stringParameters.Add(stringParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case FloatParameterField floatParameterField:
                    floatParameters.Add(floatParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case IntParameterField intParameterField:
                    intParameters.Add(intParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case BoolParameterField boolParameterField:
                    boolParameters.Add(boolParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void RemoveParameter(ExposedParameter param) {
            switch (param) {
                case StringParameterField stringParameterField:
                    stringParameters.Remove(stringParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case FloatParameterField floatParameterField:
                    floatParameters.Remove(floatParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case IntParameterField intParameterField:
                    intParameters.Remove(intParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                case BoolParameterField boolParameterField:
                    boolParameters.Remove(boolParameterField);
                    EditorUtility.SetDirty(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isMulti, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortCapacity = isMulti ? "Multi" : "Single",
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

}