#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    public class WorldGraphManager : MonoBehaviour {
        private void Awake() {
            #if UNITY_EDITOR
            ResourceManager.ExceptionHandler = (handle, exception) => {
                if (exception.GetType() == typeof(InvalidKeyException) || exception.GetType() == typeof(OperationException)) { }
            };
            #endif
        }

        private void Start() {
            foreach (var handle in WorldGraph.Instance.sceneHandles) {
                Debug.Log(handle.HandleName);
            }
        }
    }

}