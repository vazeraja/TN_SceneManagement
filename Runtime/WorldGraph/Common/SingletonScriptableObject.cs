/************************************************************
 * Better Singleton by David Darias
 * Use as you like - credit where due would be appreciated :D
 * Licence: WTFPL V2, Dec 2014
 * Tested on Unity v5.6.0 (should work on earlier versions)
 * 03/02/2017 - v1.1 
 * **********************************************************/

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public class SingletonScriptableObject<T> : BehaviourScriptableObject where T : BehaviourScriptableObject {
        //Private reference to the scriptable object
        private static T _instance;
        private static bool _instantiated;

        public static T Instance {
            get {
                if (_instantiated) return _instance;
                string singletonName = typeof(T).Name;
                //Look for the singleton on the resources folder
                var assets = Resources.LoadAll<T>("");
                if (assets.Length > 1)
                    Debug.LogError("Found multiple " + singletonName +
                                   "s on the resources folder. It is a Singleton ScriptableObject, there should only be one.");
                if (assets.Length == 0) {
                    _instance = CreateInstance<T>();
                    Debug.LogError("Could not find a " + singletonName +
                                   " on the resources folder. It was created at runtime, therefore it will not be visible on the assets folder and it will not persist.");
                }
                else {
                    _instance = assets[0];
                }

                _instantiated = true;
                //Create a new game object to use as proxy for all the MonoBehaviour methods
                var baseObject = new GameObject(singletonName);
                //Deactivate it before adding the proxy component. This avoids the execution of the Awake method when the the proxy component is added.
                baseObject.SetActive(false);
                //Add the proxy, set the instance as the parent and move to DontDestroyOnLoad scene
                var proxy = baseObject.AddComponent<BehaviourProxy>();
                proxy.Parent = _instance;
                Behaviour = proxy;
                DontDestroyOnLoad(Behaviour.gameObject);
                //Activate the proxy. This will trigger the MonoBehaviourAwake. 
                proxy.gameObject.SetActive(true);
                return _instance;
            }
        }

        //Use this reference to call MonoBehaviour specific methods (for example StartCoroutine)
        protected static MonoBehaviour Behaviour;

        public static void BuildSingletonInstance() {
            BehaviourScriptableObject i = Instance;
        }

        private void OnDestroy() {
            _instantiated = false;
        }
    }

    #if UNITY_EDITOR
    // //Empty custom editor to have cleaner UI on the editor.
    // [CustomEditor(typeof(BehaviourProxy))]
    // public class BehaviourProxyEditor : Editor {
    //     public override void OnInspectorGUI() { }
    // }
    #endif

    public class BehaviourProxy : MonoBehaviour {
        public IBehaviour Parent;

        public void Awake() {
            if (Parent != null) Parent.MonoBehaviourAwake();
        }

        public void Start() {
            if (Parent != null) Parent.Start();
        }

        public void Update() {
            if (Parent != null) Parent.Update();
        }

        public void FixedUpdate() {
            if (Parent != null) Parent.FixedUpdate();
        }
    }

    public interface IBehaviour {
        void MonoBehaviourAwake();
        void Start();
        void Update();
        void FixedUpdate();
    }

    public class BehaviourScriptableObject : ScriptableObject, IBehaviour {
        public void Awake() {
            ScriptableObjectAwake();
        }

        protected virtual void ScriptableObjectAwake() { }
        public virtual void MonoBehaviourAwake() { }
        public virtual void Start() { }
        public virtual void Update() { }
        public virtual void FixedUpdate() { }
    }

}