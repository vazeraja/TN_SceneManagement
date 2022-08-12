using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Search;

namespace ThunderNut.SceneManagement.Editor {
    internal class DefaultWorldGraphWindow : WGEditorWindow {
        public static bool ShowWorldGraphEditorWindow(string path) {
            string guid = AssetDatabase.AssetPathToGUID(path);
            
            foreach (var w in Resources.FindObjectsOfTypeAll<WGEditorWindow>())
            {
                if (w.selectedGuid == guid)
                {
                    w.Focus();
                    return true;
                }
            }
            
            var window = CreateWindow<DefaultWorldGraphWindow>(typeof(WGEditorWindow), typeof(SceneView));
            window.minSize = new Vector2(1200, 600);
            window.Initialize(guid);
            window.Focus();

            return true;
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as WorldGraph;
            var path = AssetDatabase.GetAssetPath(instanceID);

            if (asset == null || !AssetDatabase.GetAssetPath(asset).Contains("WorldGraph"))
                return false;
            
            return ShowWorldGraphEditorWindow(path);
        }

    }
}