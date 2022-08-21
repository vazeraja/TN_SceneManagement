using System;
using System.Collections.Generic;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : SingletonScriptableObject<WorldGraph> {
        
        public List<SceneHandle> sceneHandles;

        protected override void ScriptableObjectAwake() => Debug.Log($"{GetType().Name} created.");
    }

}