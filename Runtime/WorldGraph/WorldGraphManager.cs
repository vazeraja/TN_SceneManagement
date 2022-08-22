#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Exceptions;

namespace ThunderNut.SceneManagement {

    public class WorldGraphManager : MonoBehaviour {
        public WorldGraph worldGraph;

        private void Awake() {
            #if UNITY_EDITOR
            ResourceManager.ExceptionHandler = (handle, exception) => {
                if (exception.GetType() == typeof(InvalidKeyException) || exception.GetType() == typeof(OperationException)) { }
            };
            #endif

            Debug.Log(worldGraph.sceneHandles.Count);
        }
    }

}