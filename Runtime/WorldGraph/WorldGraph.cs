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
        private SceneHandle activeSceneHandle;
        
        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        [SerializeField, SerializeReference]
        private List<StringParameterField> stringParameters;
        [SerializeField, SerializeReference]
        private List<FloatParameterField> floatParameters;
        [SerializeField, SerializeReference]
        private List<IntParameterField> intParameters;
        [SerializeField, SerializeReference]
        private List<BoolParameterField> boolParameters;

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

        #if UNITY_EDITOR
        public ExposedParameter CreateParameter(ParameterType type) {
            switch (type) {
                case ParameterType.String:
                    var stringParameter = (StringParameterField) CreateInstance(typeof(StringParameterField));
                    stringParameters.Add(stringParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(stringParameter, this);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    return stringParameter;
                case ParameterType.Float:
                    var floatParameter = (FloatParameterField) CreateInstance(typeof(FloatParameterField));
                    floatParameters.Add(floatParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(floatParameter, this);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    return floatParameter;
                case ParameterType.Int:
                    var intParameter = (IntParameterField) CreateInstance(typeof(IntParameterField));
                    intParameters.Add(intParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(intParameter, this);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    return intParameter;
                case ParameterType.Bool:
                    var boolParameter = (BoolParameterField) CreateInstance(typeof(BoolParameterField));
                    boolParameters.Add(boolParameter);

                    if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(boolParameter, this);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    return boolParameter;
                default:
                    return null;
            }
        }
        // public ExposedParameter CreateParameter(ParameterType type, string attrName) {
        //     // switch (type) {
        //     //     case ParameterType.String:
        //     //         var stringParameter = CreateInstance<StringParameterField>();
        //     //         stringParameter.Name = attrName;
        //     //         stringParameter.Reference = attrName;
        //     //         stringParameters.Add(stringParameter);
        //     //         EditorUtility.SetDirty(this);
        //     //         return stringParameter;
        //     //     case ParameterType.Float:
        //     //         var floatParameter = CreateInstance<FloatParameterField>();
        //     //         floatParameter.Name = attrName;
        //     //         floatParameter.Reference = attrName;
        //     //         floatParameters.Add(floatParameter);
        //     //         EditorUtility.SetDirty(this);
        //     //         return floatParameter;
        //     //     case ParameterType.Int:
        //     //         var intParameter = CreateInstance<IntParameterField>();
        //     //         intParameter.Name = attrName;
        //     //         intParameter.Reference = attrName;
        //     //         intParameters.Add(intParameter);
        //     //         EditorUtility.SetDirty(this);
        //     //         return intParameter;
        //     //     case ParameterType.Bool:
        //     //         var boolParameter = CreateInstance<BoolParameterField>();
        //     //         boolParameter.Name = attrName;
        //     //         boolParameter.Reference = attrName;
        //     //         boolParameters.Add(boolParameter);
        //     //         EditorUtility.SetDirty(this);
        //     //         return boolParameter;
        //     //     default:
        //     //         return null;
        //     // }
        // }

        public void RemoveParameter(ExposedParameter parameter) {
            switch (parameter) {
                case StringParameterField stringParameterField:
                    stringParameters.Remove(stringParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    break;
                case FloatParameterField floatParameterField:
                    floatParameters.Remove(floatParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    break;
                case IntParameterField intParameterField:
                    intParameters.Remove(intParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    break;
                case BoolParameterField boolParameterField:
                    boolParameters.Remove(boolParameterField);

                    AssetDatabase.RemoveObjectFromAsset(parameter);
                    AssetDatabase.SaveAssets();

                    EditorUtility.SetDirty(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            AssetDatabase.SaveAssets();
        }

        private SceneHandle CreateSceneHandle(Type type) {
            SceneHandle newHandle = (SceneHandle) CreateInstance(type);
            newHandle.name = type.Name;
            newHandle.GUID = Guid.NewGuid().ToString();

            AddSceneHandle(newHandle);
            return newHandle;
        }

        private void AddSceneHandle(SceneHandle handle) {
            sceneHandles.Add(handle);
            EditorUtility.SetDirty(this);
        }

        private void RemoveSceneHandle(SceneHandle handle) {
            sceneHandles.Remove(handle);
            EditorUtility.SetDirty(this);
        }

        public void AddChild(SceneHandle parent, SceneHandle child) {
            parent.children.Add(child);
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(SceneHandle parent, SceneHandle child) {
            parent.children.Remove(child);
            EditorUtility.SetDirty(this);
        }

        public SceneHandle CreateSubAsset(Type type) {
            var newHandle = CreateSceneHandle(type);

            if (!Application.isPlaying) AssetDatabase.AddObjectToAsset(newHandle, this);
            AssetDatabase.SaveAssets();

            return newHandle;
        }

        public void RemoveSubAsset(SceneHandle handle) {
            RemoveSceneHandle(handle);
            AssetDatabase.RemoveObjectFromAsset(handle);
            AssetDatabase.SaveAssets();
        }

        public static IEnumerable<SceneHandle> GetChildren(SceneHandle parent) {
            return parent.children.Any() ? parent.children : Enumerable.Empty<SceneHandle>();
        }

        #endif
    }

}