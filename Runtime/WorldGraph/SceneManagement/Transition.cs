using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class ConditionValueBase { }

    public abstract class ConditionValue<T> : ConditionValueBase {
        public T Value;
    }

    [Serializable]
    public class StringCondition : ConditionValue<string> {
        public StringParamOptions stringOptions;

        public StringCondition() {
            Value = "string";
        }
    }

    [Serializable]
    public class FloatCondition : ConditionValue<float> {
        public FloatParamOptions floatOptions;

        public FloatCondition() {
            Value = 0f;
        }
    }

    [Serializable]
    public class IntCondition : ConditionValue<int> {
        public IntParamOptions intOptions;

        public IntCondition() {
            Value = 0;
        }
    }

    [Serializable]
    public class BoolCondition : ConditionValue<bool> {
        public BoolParamOptions boolOptions;

        public BoolCondition() {
            Value = true;
        }
    }

    [Serializable]
    public class Condition {
        [SerializeReference] public ExposedParameter Parameter;
        [SerializeReference] public ConditionValueBase Value;

        public Func<bool> FloatIsGreaterThan() => () => ((FloatParameterField) Parameter).Value > ((FloatCondition) Value).Value;
        public Func<bool> FloatIsLessThan() => () => ((FloatParameterField) Parameter).Value < ((FloatCondition) Value).Value;

        public Func<bool> BoolIsTrue() => () => ((BoolParameterField) Parameter).Value;
        public Func<bool> BoolIsFalse() => () => !((BoolParameterField) Parameter).Value;

        public Func<bool> IntIsGreaterThan() => () => ((IntParameterField) Parameter).Value > ((IntCondition) Value).Value;
        public Func<bool> IntIsLessThan() => () => ((IntParameterField) Parameter).Value < ((IntCondition) Value).Value;
        public Func<bool> IntIsEqual() => () => ((IntParameterField) Parameter).Value == ((IntCondition) Value).Value;
        public Func<bool> IntNotEqual() => () => ((IntParameterField) Parameter).Value != ((IntCondition) Value).Value;

        public Func<bool> StringIsEqual() => () => ((StringParameterField) Parameter).Value == ((StringCondition) Value).Value;
        public Func<bool> StringNotEqual() => () => ((StringParameterField) Parameter).Value != ((StringCondition) Value).Value;
    }

    [Serializable]
    public class TransitionBase { }

    [Serializable]
    public class Transition : TransitionBase, ISerializationCallbackReceiver {
        public WorldGraph WorldGraph;

        public string OutputNodeGUID;
        public string InputNodeGUID;
        public SceneHandle OutputNode;
        public SceneHandle InputNode;

        public List<Condition> Conditions;
  
        public override string ToString() {
            return $"{OutputNode.HandleName} ---> {InputNode.HandleName}";
        }

        public void OnBeforeSerialize() {
            // foreach (var condition in Conditions) {
            //     if (condition.Parameter == null) continue;
            //     var match = WorldGraph.allParameters.Find(param => param.GUID == condition.Parameter.GUID);
            //     condition.Parameter = match;
            // }
        }

        public void OnAfterDeserialize() {
        }
    }

}