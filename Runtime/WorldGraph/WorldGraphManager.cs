﻿#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using UnityEngine;

#if UNITY_2022_1_OR_NEWER
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using UnityEngine.ResourceManagement.Exceptions;
#endif

using UnityEngine.SceneManagement; 

namespace ThunderNut.SceneManagement {

    public class WorldGraphManager : MonoBehaviour {
        private void Awake() {
            #if UNITY_EDITOR && UNITY_2022_1_OR_NEWER
            ResourceManager.ExceptionHandler = (handle, exception) => {
                if (exception.GetType() == typeof(InvalidKeyException) || exception.GetType() == typeof(OperationException)) { }
            };
            #endif
        }

        private void Start() {
            SceneHandle sceneHandle = WorldGraph.Instance.sceneHandles.Find(handle => handle.HandleName == "Boss Battle 1");
            SceneManager.LoadScene(sceneHandle.scene.ScenePath);
        }
    }

}