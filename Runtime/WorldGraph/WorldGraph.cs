using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph"), Serializable]
    public class WorldGraph : ScriptableObject, ISerializationCallbackReceiver {
        public string selectedItem;
        [SearchObject(typeof(MyDemoScriptableObject))]
        public MyDemoScriptableObject DemoScriptableObject;

        public List<BaseSceneHandle> SceneHandles;

        private void OnEnable() {
        }
        private void OnDisable() {
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }
    }

}