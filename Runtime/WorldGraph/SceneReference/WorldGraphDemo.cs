using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    public class WorldGraphDemo : MonoBehaviour {
        public BaseSceneHandle baseSceneHandle;

        public WorldGraph worldGraph;
        
        [SearchObject(typeof(MyDemoScriptableObject))]
        public MyDemoScriptableObject DemoScriptableObject;
        
        private void Awake() {
            //SceneManager.LoadScene(baseSceneHandle.scene.sceneIndex);
            worldGraph.SceneHandle.passages.ForEach(Debug.Log);
        }
    }
}