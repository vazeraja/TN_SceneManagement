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
        private Action onStateTransition;

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        [SerializeReference] public List<ExposedParameterViewData> ExposedParameterViewDatas;
        [SerializeReference] public List<ExposedParameter> allParameters;
        [SerializeReference] public List<Transition> transitions;


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
                    switch (condition.Value) {
                        case StringCondition stringCondition:
                            switch (stringCondition.stringOptions) {
                                case StringParamOptions.Equals:
                                    conditionsToMeet.Add(condition.StringIsEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case StringParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.StringNotEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case FloatCondition floatCondition:
                            switch (floatCondition.floatOptions) {
                                case FloatParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.FloatIsGreaterThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case FloatParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.FloatIsLessThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case IntCondition intCondition:
                            switch (intCondition.intOptions) {
                                case IntParamOptions.Equals:
                                    conditionsToMeet.Add(condition.IntIsEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.NotEquals:
                                    conditionsToMeet.Add(condition.IntNotEqual());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.GreaterThan:
                                    conditionsToMeet.Add(condition.IntIsGreaterThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case IntParamOptions.LessThan:
                                    conditionsToMeet.Add(condition.IntIsLessThan());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
                        case BoolCondition boolCondition:
                            switch (boolCondition.boolOptions) {
                                case BoolParamOptions.True:
                                    conditionsToMeet.Add(condition.BoolIsTrue());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                case BoolParamOptions.False:
                                    conditionsToMeet.Add(condition.BoolIsFalse());
                                    allConditionsLookupTable[transition] = conditionsToMeet;
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }

                            break;
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

        public void SetString(string name, string value) {
            var match = (StringParameterField) allParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetFloat(string name, float value) {
            var match = (FloatParameterField)allParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }
 
        public void SetInt(string name, int value) {
            var match = (IntParameterField) allParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        public void SetBool(string name, bool value) {
            var match = (BoolParameterField) allParameters.ToList().Find(param => param.Reference == name);
            match.Value = value;
        }

        #if UNITY_EDITOR

        public void SaveAssetsAndSetDirty() {
            AssetDatabase.SaveAssets();
            EditorUtility.SetDirty(this);
        }

        public ExposedParameter CreateParameter(string type) {
            switch (type) {
                case "String":
                    var stringParameter = new StringParameterField();
                    allParameters.Add(stringParameter);
                    return stringParameter;
                case "Float":
                    var floatParameter = new FloatParameterField();
                    allParameters.Add(floatParameter);
                    return floatParameter;
                case "Int":
                    var intParameter = new IntParameterField();
                    allParameters.Add(intParameter);
                    return intParameter;
                case "Bool":
                    var boolParameter = new BoolParameterField();
                    allParameters.Add(boolParameter);
                    return boolParameter;
                default:
                    return null;
            }
        }

        public void RemoveParameter(ExposedParameter parameter) {
            switch (parameter) {
                case StringParameterField stringParameterField:
                    allParameters.Remove(stringParameterField);
                    break;
                case FloatParameterField floatParameterField:
                    allParameters.Remove(floatParameterField);
                    break;
                case IntParameterField intParameterField:
                    allParameters.Remove(intParameterField);
                    break;
                case BoolParameterField boolParameterField:
                    allParameters.Remove(boolParameterField);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Transition CreateTransition(SceneHandle output, SceneHandle input) {
            Transition edge = new Transition {
                WorldGraph = this,
                OutputNodeGUID = output.GUID,
                OutputNode = output,
                InputNodeGUID = input.GUID,
                InputNode = input
            };
            transitions.Add(edge);
            return edge;
        }


        public void RemoveTransition(Transition edge) {
            transitions.Remove(edge);
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