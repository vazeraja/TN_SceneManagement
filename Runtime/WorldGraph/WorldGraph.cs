using System;
using System.Collections.Generic;
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
        
        public SerializableDictionary<string, string> stringParametersDict = new SerializableDictionary<string, string>();
        public SerializableDictionary<string, float> floatParametersDict = new SerializableDictionary<string, float>();
        public SerializableDictionary<string, int> intParametersDict = new SerializableDictionary<string, int>();
        public SerializableDictionary<string, bool> boolParametersDict = new SerializableDictionary<string, bool>();


        public void ChangeScene() {
            activeSceneHandle.ChangeToScene();
        }

        public void ClearDictionaries() {
            stringParametersDict.Clear();
            floatParametersDict.Clear();
            intParametersDict.Clear();
            boolParametersDict.Clear();
        }

        public void RegisterParameters(object obj) {
            var fieldsWithAttribute = WGReflectionHelper.GetFieldInfosWithAttribute(obj, typeof(ParameterFilterAttribute));
            foreach (FieldInfo field in fieldsWithAttribute) {
                var attribute = (ParameterFilterAttribute) field.GetCustomAttribute(typeof(ParameterFilterAttribute), true);
                object fieldValue = field.GetValue(obj);

                Debug.Log($"Key: {attribute.Name} , Value: {fieldValue}");

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
                Debug.LogWarning($"Parameter {key} overriding previous value");
                stringParametersDict[key] = value;
            }
            else {
                stringParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, float value) {
            if (floatParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Parameter {key} overriding previous value");
                floatParametersDict[key] = value;
            }
            else {
                floatParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, int value) {
            if (intParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Parameter {key} overriding previous value");
                intParametersDict[key] = value;
            }
            else {
                intParametersDict.Add(key, value);
            }
        }

        private void RegisterParameter(string key, bool value) {
            if (boolParametersDict.ContainsKey(key)) {
                Debug.LogWarning($"Parameter {key} overriding previous value");
                boolParametersDict[key] = value;
            }
            else {
                boolParametersDict.Add(key, value);
            }
        }

        #region Editor

        #if UNITY_EDITOR
        public SceneHandle CreateSubAsset(Type type) {
            SceneHandle newHandle = (SceneHandle) CreateInstance(type);
            newHandle.name = type.Name;
            newHandle.guid = GUID.Generate().ToString();

            sceneHandles.Add(newHandle);

            Undo.RecordObject(this, name);
            if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(newHandle, this);
            Undo.RegisterCreatedObjectUndo(newHandle, name);

            AssetDatabase.SaveAssets();

            return newHandle;
        }

        public void RemoveSubAsset(SceneHandle handle) {
            Undo.RecordObject(this, "Resolution Tree");

            sceneHandles.Remove(handle);

            Undo.DestroyObjectImmediate(handle);
            AssetDatabase.SaveAssets();
        }
        #endif

        #endregion
    }

}