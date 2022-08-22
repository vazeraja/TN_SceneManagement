using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Basic/CutsceneHandle", "Cutscene")]
    public class CutsceneHandle : SceneHandle {
        public override Color HandleColor => Color.green;
    }

}