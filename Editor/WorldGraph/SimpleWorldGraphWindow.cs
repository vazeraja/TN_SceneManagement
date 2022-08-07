using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEditor.Search;

namespace ThunderNut.SceneManagement.Editor {
    public class SimpleWorldGraphWindow : WGEditorWindow {
        private WorldGraph tmpGraph;

        [MenuItem("World Graph/World Graph")]
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
            window.InitializeGraph(window.tmpGraph);

            window.Show();
            return window;
        }

        protected override void OnDestroy() {
            graphView?.Dispose();
            DestroyImmediate(tmpGraph);
        }

        protected override void InitializeWindow(WorldGraph graph) {
            Debug.Log("DefaultWorldGraphWindow: InitializeWindow()");
        }

        protected override void InitializeGraphView(WGGraphView view) {
            Debug.Log("DefaultWorldGraphWindow: InitializeGraphView()");
        }
    }
}