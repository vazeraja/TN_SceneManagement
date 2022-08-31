using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace ThunderNut.SceneManagement {
    
    [AddComponentMenu("ThunderNut/Graph/WorldGraph")]
    [DisallowMultipleComponent]
    public class WorldGraph : MonoBehaviour {
        
        public List<SceneHandle> sceneHandles;

        public string settingA;
        public string settingB;
        public string settingC;
        public string settingD;
        public string settingE;
    }

}