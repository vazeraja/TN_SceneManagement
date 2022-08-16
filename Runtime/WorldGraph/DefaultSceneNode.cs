using UnityEngine;

namespace ThunderNut.SceneManagement {
    [CreateAssetMenu(fileName = "SceneHandle", menuName = "World Graph/Default Scene Handle")]
    public class DefaultSceneNode : BaseSceneNode {
        protected override void ForceSwitchToScene() {
            base.ForceSwitchToScene();
            Debug.Log("");
        }
    }
}