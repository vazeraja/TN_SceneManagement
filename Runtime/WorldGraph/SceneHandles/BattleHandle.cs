using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Special/BattleHandle", "Battle")]
    public class BattleHandle : SceneHandle {
        public override Color HandleColor => Color.red;
    }

}