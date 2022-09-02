using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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

        private readonly HashSet<string> stringParameters = new HashSet<string>();

        public void ChangeScene() {
            activeSceneHandle.ChangeToScene();
        }

        private void RegisterParameter(string param) {
            if (!stringParameters.Add(param)) Debug.LogError($"{param} with that name already exists");
        }

        public void RegisterParameters(object obj) {
            var fieldsWithAttribute =
                WGReflectionHelper.GetFieldInfosWithAttribute(obj, typeof(ParameterFilterAttribute));
            foreach (FieldInfo field in fieldsWithAttribute) {
                object s = field.GetValue(obj);
                RegisterParameter(s.ToString());
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