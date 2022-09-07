﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public abstract class ExposedParameter {
        [SerializeField] private string m_Name = "ExposedProperty";
        public string Name {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField] private string m_Reference = "_ExposedProperty";
        public string Reference {
            get => m_Reference;
            set => m_Reference = value;
        }
        [SerializeField] private bool m_Exposed = true;
        public bool Exposed {
            get => m_Exposed;
            set => m_Exposed = value;
        }
    }

    [Serializable]
    public abstract class ParameterField<TValueType> : ExposedParameter {
        [SerializeField] private TValueType m_Value;
        public TValueType Value {
            get => m_Value;
            set => m_Value = value;
        }
    }

    [Serializable]
    public class StringParameterField : ParameterField<string> {
        public StringParameterField() {
            Name = "StringParameter";
            Reference = "_StringParameter";
        }

        public StringParameterField(string name) : this() {
            Name = name;
        }

        public StringParameterField(string name, string value) : this() {
            Name = name;
            Value = value;
        }
    }

    [Serializable]
    public class FloatParameterField : ParameterField<float> {
        public FloatParameterField() {
            Name = "FloatParameter";
            Reference = "_FloatParameter";
        }

        public FloatParameterField(string name) : this() {
            Name = name;
        }

        public FloatParameterField(string name, float value) : this() {
            Name = name;
            Value = value;
        }
    }

    [Serializable]
    public class IntParameterField : ParameterField<int> {
        public IntParameterField() {
            Name = "IntParameter";
            Reference = "_IntParameter";
        }

        public IntParameterField(string name) : this() {
            Name = name;
        }

        public IntParameterField(string name, int value) : this() {
            Name = name;
            Value = value;
        }
    }

    [Serializable]
    public class BoolParameterField : ParameterField<bool> {
        public BoolParameterField() {
            Name = "BoolParameter";
            Reference = "_BoolParameter";
        }

        public BoolParameterField(string name) : this() {
            Name = name;
        }

        public BoolParameterField(string name, bool value) : this() {
            Name = name;
            Value = value;
        }
    }

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : ScriptableObject {
        public List<SceneHandle> sceneHandles;

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        private SceneHandle activeSceneHandle;

        public List<StringParameterField> stringParameters = new List<StringParameterField>();
        public List<FloatParameterField> floatParameters = new List<FloatParameterField>();
        public List<IntParameterField> intParameters = new List<IntParameterField>();
        public List<BoolParameterField> boolParameters = new List<BoolParameterField>();

        // --------------------- Key: Represents parameter name | Value: Represents parameter value ---------------------
        public SerializableDictionary<string, string> stringParametersDict = new SerializableDictionary<string, string>();
        public SerializableDictionary<string, float> floatParametersDict = new SerializableDictionary<string, float>();
        public SerializableDictionary<string, int> intParametersDict = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, bool> boolParametersDict = new SerializableDictionary<string, bool>();

        public void ChangeScene() {
            activeSceneHandle.ChangeToScene();
        }

        public void SetString(string name, string value) {
            stringParametersDict[name] = value;
        }

        public void SetFloat(string name, float value) {
            floatParametersDict[name] = value;
        }

        public void SetInt(string name, int value) {
            intParametersDict[name] = value;
        }

        public void SetBool(string name, bool value) {
            boolParametersDict[name] = value;
        }

        public void RegisterParameters(object obj) {
            var fieldsWithAttribute = WGReflectionHelper.GetFieldInfosWithAttribute(obj, typeof(ParameterAttribute));

            foreach (FieldInfo field in fieldsWithAttribute) {
                var attribute = (ParameterAttribute) field.GetCustomAttribute(typeof(ParameterAttribute), true);
                object fieldValue = field.GetValue(obj);

                // Debug.Log($"Key: {attribute.Name} , Value: {fieldValue}");

                switch (attribute.Type) {
                    case ParameterType.StringParam:
                        RegisterParameter(attribute.Name, (string) fieldValue);
                        break;
                    case ParameterType.FloatParam:
                        RegisterParameter(attribute.Name, (float) fieldValue);
                        break;
                    case ParameterType.IntParam:
                        RegisterParameter(attribute.Name, (int) fieldValue);
                        break;
                    case ParameterType.BoolParam:
                        RegisterParameter(attribute.Name, (bool) fieldValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void RegisterParameter(string key, string value) {
            if (stringParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Key {key} overriding previous value");
                stringParametersDict[key] = value;
            }
            else {
                stringParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, float value) {
            if (floatParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Key {key} overriding previous value");
                floatParametersDict[key] = value;
            }
            else {
                floatParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, int value) {
            if (intParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Key {key} overriding previous value");
                intParametersDict[key] = value;
            }
            else {
                intParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, bool value) {
            if (boolParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Key {key} overriding previous value");
                boolParametersDict[key] = value;
            }
            else {
                boolParametersDict.Add(key, value);
            }
        }

        public void ClearAllParameters() {
            stringParameters.Clear();
            floatParameters.Clear();
            intParameters.Clear();
            boolParameters.Clear();
        }

        public StringParameterField CreateStringParameter() {
            var param = new StringParameterField();
            stringParameters.Add(param);
            return param;
        }

        public FloatParameterField CreateFloatParameter() {
            var param = new FloatParameterField();
            floatParameters.Add(param);
            return param;
        }

        public IntParameterField CreateIntParameter() {
            var param = new IntParameterField();
            intParameters.Add(param);
            return param;
        }

        public BoolParameterField CreateBoolParameter() {
            var param = new BoolParameterField();
            boolParameters.Add(param);
            return param;
        }

        public SceneHandle CreateSceneHandle(Type type) {
            SceneHandle newHandle = (SceneHandle) CreateInstance(type);
            newHandle.name = type.Name;
            newHandle.guid = GUID.Generate().ToString();

            AddSceneHandle(newHandle);
            return newHandle;
        }

        public void AddSceneHandle(SceneHandle handle) {
            sceneHandles.Add(handle);
        }

        public void RemoveSceneHandle(SceneHandle handle) {
            sceneHandles.Remove(handle);
        }

        public void AddChild(SceneHandle parent, SceneHandle child) {
            parent.children.Add(child);
        }

        public void RemoveChild(SceneHandle parent, SceneHandle child) {
            parent.children.Remove(child);
        }

        #region Editor

        #if UNITY_EDITOR
        public SceneHandle CreateSubAsset(Type type) {
            var newHandle = CreateSceneHandle(type);

            if (!Application.isPlaying)
                AssetDatabase.AddObjectToAsset(newHandle, this);

            AssetDatabase.SaveAssets();

            return newHandle;
        }

        public void RemoveSubAsset(SceneHandle handle) {
            RemoveSceneHandle(handle);
            AssetDatabase.RemoveObjectFromAsset(handle);
            AssetDatabase.SaveAssets();
        }

        public static IEnumerable<SceneHandle> GetChildren(SceneHandle parent) {
            return parent.children.Count != 0 ? parent.children : Enumerable.Empty<SceneHandle>();
        }

        #endif

        #endregion
    }

}