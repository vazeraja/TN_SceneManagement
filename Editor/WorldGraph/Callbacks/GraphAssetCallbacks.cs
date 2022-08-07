using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class GraphAssetCallbacks {
        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as WorldGraph;
            if (asset == null || !AssetDatabase.GetAssetPath(asset).Contains("WorldGraph"))
                return false;

            var window = EditorWindow.GetWindow<SimpleWorldGraphWindow>();
            window.InitializeGraph(asset);
            window.minSize = new Vector2(1200, 600);
            return true;
        }
    }
}