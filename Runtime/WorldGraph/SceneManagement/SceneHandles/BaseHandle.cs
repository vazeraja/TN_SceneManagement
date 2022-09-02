using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Core/BaseHandle", "BaseNode")]
    public class BaseHandle : SceneHandle {
        public override Color HandleColor => Color.yellow;
        
        public override void ChangeToScene() {
            
        }
    }

}