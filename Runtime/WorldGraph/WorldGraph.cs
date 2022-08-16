using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph"), Serializable]
    public class WorldGraph : ScriptableObject, ISerializationCallbackReceiver {
        public string selectedItem;

        [SearchObject(typeof(MyDemoScriptableObject))]
        public MyDemoScriptableObject DemoScriptableObject;

        public event Action onEnabled;
        [NonSerialized] private bool _isEnabled = false;
        public bool isEnabled {
            get => _isEnabled;
            private set => _isEnabled = value;
        }

        private void OnEnable() {
            if (isEnabled)
                OnDisable();

            isEnabled = true;
            onEnabled?.Invoke();
        }

        private void OnDisable() {
            isEnabled = false;
        }

        public void OnAssetCreated([CallerMemberName]
            string callerName = "") {
            Debug.Log($"WorldGraph: Asset Created. " + "Called By: " + callerName);
        }

        public void OnAssetDeleted([CallerMemberName]
            string callerName = "") {
            Debug.Log("WorldGraph: Asset Deleted. " + "Called By: " + callerName);
        }

        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize() { }
    }

}