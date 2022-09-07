using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Basic/CutsceneHandle", "Cutscene")]
    public class CutsceneHandle : SceneHandle {
        protected override Color HandleColor => Color.red;
        
        public override void ChangeToScene() {
            
        }
    }

}