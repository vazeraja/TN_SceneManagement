using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

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

        #region Parameters Dict Code (Might Discard)

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
            var fieldsWithAttribute = WGReflectionHelper.GetFieldInfosWithAttribute(obj, typeof(ExposedParameterAttribute));

            foreach (FieldInfo field in fieldsWithAttribute) {
                var attribute = (ExposedParameterAttribute) field.GetCustomAttribute(typeof(ExposedParameterAttribute), true);
                object fieldValue = field.GetValue(obj);

                // Debug.Log($"Key: {attribute.Name} , Value: {fieldValue}");

                switch (attribute.Type) {
                    case ParameterType.String:
                        RegisterParameter(attribute.Name, (string) fieldValue);
                        break;
                    case ParameterType.Float:
                        RegisterParameter(attribute.Name, (float) fieldValue);
                        break;
                    case ParameterType.Int:
                        RegisterParameter(attribute.Name, (int) fieldValue);
                        break;
                    case ParameterType.Bool:
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

        #endregion

        public void ClearAllParameters() {
            stringParameters.Clear();
            floatParameters.Clear();
            intParameters.Clear();
            boolParameters.Clear();
        }

        public ExposedParameter CreateParameter(ParameterType type) {
            switch (type) {
                case ParameterType.String:
                    var stringParameter = new StringParameterField();
                    stringParameters.Add(stringParameter);
                    return stringParameter;
                case ParameterType.Float:
                    var floatParameter = new FloatParameterField();
                    floatParameters.Add(floatParameter);
                    return floatParameter;
                case ParameterType.Int:
                    var intParameter = new IntParameterField();
                    intParameters.Add(intParameter);
                    return intParameter;
                case ParameterType.Bool:
                    var boolParameter = new BoolParameterField();
                    boolParameters.Add(boolParameter);
                    return boolParameter;
                default:
                    return null;
            }
        }
        public ExposedParameter CreateParameter(ParameterType type, string attrName) {
            switch (type) {
                case ParameterType.String:
                    var stringParameter = new StringParameterField();
                    stringParameter.Name = attrName;
                    stringParameter.Reference = attrName;
                    stringParameters.Add(stringParameter);
                    return stringParameter;
                case ParameterType.Float:
                    var floatParameter = new FloatParameterField();
                    floatParameter.Name = attrName;
                    floatParameter.Reference = attrName;
                    floatParameters.Add(floatParameter);
                    return floatParameter;
                case ParameterType.Int:
                    var intParameter = new IntParameterField();
                    intParameter.Name = attrName;
                    intParameter.Reference = attrName;
                    intParameters.Add(intParameter);
                    return intParameter;
                case ParameterType.Bool:
                    var boolParameter = new BoolParameterField();
                    boolParameter.Name = attrName;
                    boolParameter.Reference = attrName;
                    boolParameters.Add(boolParameter);
                    return boolParameter;
                default:
                    return null;
            }
        }

        private SceneHandle CreateSceneHandle(Type type) {
            SceneHandle newHandle = (SceneHandle) CreateInstance(type);
            newHandle.name = type.Name;
            newHandle.guid = GUID.Generate().ToString();

            AddSceneHandle(newHandle);
            return newHandle;
        }

        private void AddSceneHandle(SceneHandle handle) {
            sceneHandles.Add(handle);
        }

        private void RemoveSceneHandle(SceneHandle handle) {
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