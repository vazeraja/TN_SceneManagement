using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class ConditionValue {
        public string StringValue;
        public float FloatValue;
        public int IntValue;
        public bool BoolValue;
    }

    [Serializable]
    public class Condition {
        public ExposedParameter Parameter;
        public ConditionValue Value;

        public Func<bool> FloatIsGreaterThan(FloatParameterField parameter) => () => parameter.Value > Value.FloatValue;
        public Func<bool> FloatIsLessThan(FloatParameterField parameter) => () => parameter.Value < Value.FloatValue;

        public Func<bool> BoolIsTrue(BoolParameterField parameter) => () => parameter.Value == Value.BoolValue;
        public Func<bool> BoolIsFalse(BoolParameterField parameter) => () => parameter.Value != Value.BoolValue;

        public Func<bool> IntIsGreaterThan(IntParameterField parameter) => () => parameter.Value > Value.IntValue;
        public Func<bool> IntIsLessThan(IntParameterField parameter) => () => parameter.Value < Value.IntValue;
        public Func<bool> IntIsEqual(IntParameterField parameter) => () => parameter.Value == Value.IntValue;
        public Func<bool> IntNotEqual(IntParameterField parameter) => () => parameter.Value != Value.IntValue;
        
        public Func<bool> StringIsEqual(StringParameterField parameter) => () => parameter.Value == Value.StringValue;
        public Func<bool> StringNotEqual(StringParameterField parameter) => () => parameter.Value != Value.StringValue;

    }

    public class Transition : ScriptableObject {
        public WorldGraph WorldGraph;

        public string OutputNodeGUID;
        public string InputNodeGUID;
        public SceneHandle OutputNode;
        public SceneHandle InputNode;
        
        public List<Condition> Conditions = new List<Condition>();

        public override string ToString() {
            return $"{OutputNode.HandleName} ---> {InputNode.HandleName}";
        }
    }

}