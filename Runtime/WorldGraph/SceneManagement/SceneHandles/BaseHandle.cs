using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Core/BaseHandle", "BaseNode", false)]
    public class BaseHandle : SceneHandle {
        protected override Color HandleColor => Color.white;
        
        
        public override void ChangeToScene() {
            
        }
    }

}