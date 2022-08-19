using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : ScriptableObject, ISerializationCallbackReceiver {
        public WorldGraph worldGraph => this;
        
        public string selectedItem;
        
        [SearchObject(typeof(MyDemoScriptableObject))]
        public MyDemoScriptableObject DemoScriptableObject;
        
        public SceneHandle SceneHandle;
        private void OnEnable() {
        }
        private void OnDisable() {
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }
    }

}