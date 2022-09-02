using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    [Path("Special/BattleHandle", "Battle")]
    public class BattleHandle : SceneHandle {
        public override Color HandleColor => Color.red;

        public override void ChangeToScene() {
            SceneConnection firstConnection = sceneConnections.list.First();
            LoadSceneFromConnection(firstConnection);
        }
    }

}