using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {

    [Path("Special/BattleHandle", "Battle")]
    public class BattleHandle : SceneHandle {
        protected override Color HandleColor => Color.yellow;

        public override void ChangeToScene() {
        }
    }

}