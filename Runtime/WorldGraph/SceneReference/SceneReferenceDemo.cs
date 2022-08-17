using System;
using System.Collections;
using System.Collections.Generic;
using ThunderNut.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    public class SceneReferenceDemo : MonoBehaviour {
        public BaseSceneHandle baseSceneHandle;

        public WorldGraph worldGraph;
        

        private void Awake() {
            SceneManager.LoadScene(baseSceneHandle.scene.sceneIndex);
        }
    }
}