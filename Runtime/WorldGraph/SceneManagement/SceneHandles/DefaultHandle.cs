using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Basic/DefaultHandle", "Default")]
    public class DefaultHandle : SceneHandle {
        protected override Color HandleColor => Color.blue;
        
        public override void ChangeToScene() {
            
        }
    }

}