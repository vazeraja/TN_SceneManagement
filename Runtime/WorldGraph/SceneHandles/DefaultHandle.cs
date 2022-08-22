using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Path("Basic/DefaultHandle", "Default")]
    public class DefaultHandle : SceneHandle {
        public override Color HandleColor => Color.blue;
    }

}