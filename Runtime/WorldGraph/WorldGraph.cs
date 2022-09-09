using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    
    [Serializable]
    public class EdgeData {
        public SceneHandle BaseSceneHandle;
        public string BaseNodeGUID;
        public SceneHandle TargetSceneHandle;
        public string TargetNodeGUID;
    }
    
    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : ScriptableObject {
        public List<EdgeData> edges = new List<EdgeData>();
        
        public List<SceneHandle> sceneHandles;
        private SceneHandle activeSceneHandle;

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;

        public List<StringParameterField> stringParameters = new List<StringParameterField>();
        public List<FloatParameterField> floatParameters = new List<FloatParameterField>();
        public List<IntParameterField> intParameters = new List<IntParameterField>();
        public List<BoolParameterField> boolParameters = new List<BoolParameterField>();
        
        public void ChangeScene() {
            activeSceneHandle.ChangeToScene();
        }

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
            EditorUtility.SetDirty(this);
        }

        public void RemoveChild(SceneHandle parent, SceneHandle child) {
            parent.children.Remove(child);
            EditorUtility.SetDirty(this);
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