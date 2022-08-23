using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement; 

namespace ThunderNut.SceneManagement {

    public class WorldGraphManager : MonoBehaviour {
        private void Start() {
            SceneHandle sceneHandle = WorldGraph.Instance.sceneHandles.Find(handle => handle.HandleName == "Boss Battle 1");
            SceneManager.LoadScene(sceneHandle.scene.ScenePath);
        }
        
    }
}