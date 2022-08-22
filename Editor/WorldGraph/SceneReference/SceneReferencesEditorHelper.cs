// Public Domain. NO WARRANTIES. License: https://opensource.org/licenses/0BSD

using UnityEngine;
using UnityEditor;

namespace ThunderNut.SceneManagement {
    class SceneReferencesEditorHelper : ScriptableObject {
        [SerializeField] internal SceneAsset missingScene = null, nullScene = null;
    }
}