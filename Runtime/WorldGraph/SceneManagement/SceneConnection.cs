using System;
using System.Collections.Generic;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class SceneConnection {
        public SceneHandle exitScene;
        public string exitScenePassage;
        public SceneHandle entryScene;
        public string entryScenePassage;
    }
    
    [Serializable]
    public class SceneConnectionsList {
        public List<SceneConnection> list;
    }
    
}