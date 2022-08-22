using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : SingletonScriptableObject<WorldGraph> {
        public List<SceneHandle> sceneHandles;

        public string a;
        public string b;
        public string c;
        public string d;
        public string e;

        protected override void ScriptableObjectAwake() => Debug.Log($"{GetType().Name} created.");

        #if UNITY_EDITOR
        public SceneHandle CreateSubAsset(Type type) {
            SceneHandle newHandle = CreateInstance<SceneHandle>();
            newHandle.name = type.Name;
            newHandle.guid = GUID.Generate().ToString();
            sceneHandles.Add(newHandle);

            Undo.RecordObject(this, name);
            if (!Application.isPlaying) {
                AssetDatabase.AddObjectToAsset(newHandle, this);
            }
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
    }

}