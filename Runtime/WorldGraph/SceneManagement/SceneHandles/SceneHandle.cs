using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        public string GUID;
        public Vector2 Position;
        public List<PortData> Ports = new List<PortData>();
        protected virtual Color HandleColor => Color.white;
        public Color Color => HandleColor;

        public WorldGraph WorldGraph;
        public bool Active = true;
        public string HandleName = "";
        public SceneReference scene;
        public List<SceneHandle> children = new List<SceneHandle>();

        [SerializeField] private List<StringParameterField> stringParameters = new List<StringParameterField>();
        [SerializeField] private List<FloatParameterField> floatParameters = new List<FloatParameterField>();
        [SerializeField] private List<IntParameterField> intParameters = new List<IntParameterField>();
        [SerializeField] private List<BoolParameterField> boolParameters = new List<BoolParameterField>();

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

        public void Enter() { }
        public void Exit() { }

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
            Ports.Add(portData);
            EditorUtility.SetDirty(this);
            return portData;
        }

        public void RemovePort(PortData portData) {
            Ports.Remove(portData);
            EditorUtility.SetDirty(this);
        }
        #endif
    }

}