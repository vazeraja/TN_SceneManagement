using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Special/BattleHandle", "Battle")]
    public class BattleHandle : SceneHandle {
        public override Color HandleColor => Color.red;
        public override string HandleName => "Battle";
    }

}