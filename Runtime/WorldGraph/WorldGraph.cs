using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph", order = 0)]
    public class WorldGraph : ScriptableObject {
        public List<SceneHandle> sceneHandles;
        [SerializeField] private SceneHandle activeSceneHandle;

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private Action onStateTransition;

        [SerializeField] private List<StringParameterField> stringParameters;
        [SerializeField] private List<FloatParameterField> floatParameters;
        [SerializeField] private List<IntParameterField> intParameters;
        [SerializeField] private List<BoolParameterField> boolParameters;

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

        public List<Transition> transitions = new List<Transition>();

        private Dictionary<Transition, List<Func<bool>>> allConditionsLookupTable = new Dictionary<Transition, List<Func<bool>>>();

        public List<Transition> currentTransitions = new List<Transition>();
        private List<List<Func<bool>>> currentConditions = new List<List<Func<bool>>>();

        public void CheckTransitions() {
            for (var i = 0; i < currentConditions.Count; i++) {
                var conditionsPerTransition = currentConditions[i];
                var conditionsMet = new bool[conditionsPerTransition.Count];

                for (var index = 0; index < conditionsPerTransition.Count; index++) {
                    Func<bool> condition = conditionsPerTransition[index];
                    conditionsMet[index] = condition();
                }

                if (conditionsMet.All(x => x)) {
                    Debug.Log($"All Conditions Met for Transition: {currentTransitions[i]}");
                }
            }
        }

        public void Initialize() {
            InitializeLookupTable();
            
            foreach (var (key, value) in allConditionsLookupTable) {
                Debug.Log($"Transition: {key} with {value.Count} conditions");
            }

            currentTransitions = transitions.FindAll(t => t.OutputNode == activeSceneHandle);
            foreach (var transition in currentTransitions) {
                foreach (var pair in allConditionsLookupTable) {
                    if (pair.Key == transition) {
                        currentConditions.Add(pair.Value);
                    }
                }
            }
        }

        private void InitializeLookupTable() {
            allConditionsLookupTable = new Dictionary<Transition, List<Func<bool>>>();
            foreach (var transition in transitions) {
                var conditionsToMeet = new List<Func<bool>>();
                foreach (var condition in transition.Conditions) {
                    switch (condition.Parameter) {
                        case StringParameterField stringParameterField: {
                            switch (stringParameterField.options) {
                                case StringParamOptions.Equals:
                                    conditionsToMeet.Add(condition.StringIsEqual(stringParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case StringParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.StringNotEqual(stringParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        case FloatParameterField floatParameterField: {
                            switch (floatParameterField.options) {
                                case FloatParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.FloatIsGreaterThan(floatParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case FloatParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.FloatIsLessThan(floatParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        case IntParameterField intParameterField: {
                            switch (intParameterField.options) {
                                case IntParamOptions.Equals:
                                    conditionsToMeet.Add(condition.IntIsEqual(intParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.IntNotEqual(intParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.IntIsGreaterThan(intParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.IntIsLessThan(intParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        case BoolParameterField boolParameterField: {
                            switch (boolParameterField.options) {
                                case BoolParamOptions.True:
                                    conditionsToMeet.Add(condition.BoolIsTrue(boolParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case BoolParamOptions.False:
                                    conditionsToMeet.Add(condition.BoolIsFalse(boolParameterField));
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        }
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }
        }

        private void TransitionToState(SceneHandle nextState) {
            activeSceneHandle.Exit();
            activeSceneHandle = nextState;
            activeSceneHandle.Enter();
            onStateTransition?.Invoke();
        }

        public void SetFloat(string name, float value) {
            floatParameters.ToList().Find(param => param.Reference == name).Value = value;
        }

        public void SetBool(string name, bool value) {
            boolParameters.ToList().Find(param => param.Reference == name).Value = value;
        }

        public void SetInt(string name, int value) {
            intParameters.ToList().Find(param => param.Reference == name).Value = value;
        }

        public void SetString(string name, string value) {
            stringParameters.ToList().Find(param => param.Reference == name).Value = value;
        }

        #if UNITY_EDITOR

        public Transition CreateTransition(SceneHandle output, SceneHandle input) {
            Transition edge = CreateInstance<Transition>();
            edge.name = nameof(Transition);
            edge.WorldGraph = this;
            edge.OutputNodeGUID = output.GUID;
            edge.OutputNode = output;
            edge.InputNodeGUID = input.GUID;
            edge.InputNode = input;
            transitions.Add(edge);

            if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(edge, this);
            SaveAssetsAndSetDirty();
            return edge;
        }

        private void SaveAssetsAndSetDirty() {
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this); 
        }

        public void RemoveTransition(Transition edge) {
            transitions.Remove(edge);

            AssetDatabase.RemoveObjectFromAsset(edge);
            SaveAssetsAndSetDirty();
        }

        public ExposedParameter CreateParameter(string type) {
            switch (type) {
                case "String":
                    var stringParameter = (StringParameterField) CreateInstance(typeof(StringParameterField));
                    stringParameters.Add(stringParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(stringParameter, this);
                    SaveAssetsAndSetDirty();

                    return stringParameter;
                case "Float":
                    var floatParameter = (FloatParameterField) CreateInstance(typeof(FloatParameterField));
                    floatParameters.Add(floatParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(floatParameter, this);
                    SaveAssetsAndSetDirty();

                    return floatParameter;
                case "Int":
                    var intParameter = (IntParameterField) CreateInstance(typeof(IntParameterField));
                    intParameters.Add(intParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(intParameter, this);
                    SaveAssetsAndSetDirty();

                    return intParameter;
                case "Bool":
                    var boolParameter = (BoolParameterField) CreateInstance(typeof(BoolParameterField));
                    boolParameters.Add(boolParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(boolParameter, this);
                    SaveAssetsAndSetDirty();

                    return boolParameter;
                default:
                    return null;
            }
        }

        public void RemoveParameter(ExposedParameter parameter) {
            switch (parameter) {
                case StringParameterField stringParameterField:
                    stringParameters.Remove(stringParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    SaveAssetsAndSetDirty();

                    break;
                case FloatParameterField floatParameterField:
                    floatParameters.Remove(floatParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    SaveAssetsAndSetDirty();

                    break;
                case IntParameterField intParameterField:
                    intParameters.Remove(intParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    SaveAssetsAndSetDirty();

                    break;
                case BoolParameterField boolParameterField:
                    boolParameters.Remove(boolParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    SaveAssetsAndSetDirty();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            SaveAssetsAndSetDirty();
        }

        public void AddChild(SceneHandle parent, SceneHandle child) {
            parent.children.Add(child);
            SaveAssetsAndSetDirty();
        }

        public void RemoveChild(SceneHandle parent, SceneHandle child) {
            parent.children.Remove(child);
            SaveAssetsAndSetDirty();
        }

        public SceneHandle CreateSubAsset(Type type) {
            SceneHandle newHandle = (SceneHandle) CreateInstance(type);
            newHandle.name = type.Name;
            newHandle.GUID = Guid.NewGuid().ToString();

            sceneHandles.Add(newHandle);

            if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(newHandle, this);
            SaveAssetsAndSetDirty();

            return newHandle;
        }

        public void RemoveSubAsset(SceneHandle handle) {
            sceneHandles.Remove(handle);
            AssetDatabase.RemoveObjectFromAsset(handle);
            SaveAssetsAndSetDirty();
        }

        public static IEnumerable<SceneHandle> GetChildren(SceneHandle parent) {
            return parent.children.Any() ? parent.children : Enumerable.Empty<SceneHandle>();
        }

        #endif
    }

}