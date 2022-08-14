using UnityEngine;

namespace ThunderNut.SceneManagement {
    public class MyDemoObject : MonoBehaviour {
        public string selectedItem;

        [SearchObject(typeof(MyDemoScriptableObject))]
        public MyDemoScriptableObject targetObject;
        
    }
}