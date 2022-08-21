using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    public class WorldGraphManager : MonoBehaviour {
        public WorldGraph worldGraph;

        private void Awake() {
            Debug.Log(worldGraph.sceneHandles.Count);
            // SceneManager.LoadScene(worldGraph.sceneHandles.First().scene.sceneIndex);
        }
    }

}