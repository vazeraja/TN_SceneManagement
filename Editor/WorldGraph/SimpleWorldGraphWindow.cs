using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Search;

namespace ThunderNut.SceneManagement.Editor {
    public class SimpleWorldGraphWindow : WGEditorWindow {
        private WorldGraph tmpGraph;

        // [MenuItem("World Graph/World Graph")]
        public static WGEditorWindow ShowWindow() {
            var window = CreateWindow<SimpleWorldGraphWindow>();
            window.titleContent = new GUIContent("Default Graph");

            var position = window.position;
            position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
            window.position = position;
            window.minSize = new Vector2(1200, 600);

            // When the graph is opened from the window, we don't save the graph to disk
            window.tmpGraph = ScriptableObject.CreateInstance<WorldGraph>();
            window.tmpGraph.hideFlags = HideFlags.HideAndDontSave;
            //window.InitializeGraph(window.tmpGraph);

            window.Show();
            return window;
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as WorldGraph;
            var path = AssetDatabase.GetAssetPath(instanceID);
            var guid = AssetDatabase.AssetPathToGUID(path);
            
            if (asset == null || !AssetDatabase.GetAssetPath(asset).Contains("WorldGraph"))
                return false;

            var window = GetWindow<SimpleWorldGraphWindow>();
            window.minSize = new Vector2(1200, 600);
            window.Initialize(asset, path, guid);
            window.Focus();
            
            return true;
        }

        protected override void OnDestroy() {
        }

        protected override void InitializeWindow(WorldGraph graph) {
        }

        protected override void InitializeGraphView(WGGraphView view) {
        }
    }
}