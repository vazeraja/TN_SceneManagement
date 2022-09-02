using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class SceneReference {
        [SerializeField] private Object sceneAsset;
        [SerializeField] public string ScenePath;
    }

}