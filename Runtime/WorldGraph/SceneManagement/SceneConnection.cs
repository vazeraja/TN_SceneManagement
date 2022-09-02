using System;
using System.Collections.Generic;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class SceneConnection {
        public SceneHandle exitScene;
        public int exitScenePassage;
        public SceneHandle entryScene;
        public int entryScenePassage;
    }
    
    [Serializable]
    public class SceneConnectionsList {
        public List<SceneConnection> list;
    }
    
}