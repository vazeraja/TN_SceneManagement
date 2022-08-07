using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;

namespace ThunderNut.SceneManagement.Editor {
    [Serializable]
    public abstract class WGEditorWindow : EditorWindow {
        private const string visualTreePath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uxml";
        private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

        protected VisualElement root;
        protected WorldGraph graph;
        protected WGGraphView graphView;
        protected TwoPaneCustomControl twoPaneCustomControl;
        protected ScrollViewCustomControl scrollViewCustomControl;

        public bool isGraphLoaded => graphView != null && graphView.graph != null;
        private bool reloadWorkaround = false;

        public event Action<WorldGraph> graphLoaded;
        public event Action<WorldGraph> graphUnloaded;

        /// <summary>
        /// OnGUI() - Runs frequently - whenever clicks are registered within the editor window and when repaints happen
        /// CreateGUI() - Runs once - when the editor window is opened
        /// CreateGUI() is also very similar to OnEnable()
        /// </summary>
        protected virtual void CreateGUI() {
            InitializeRootView();
            LoadGraph();
        }

        protected virtual void OnDisable() {
            if (graph != null && graphView != null)
                graphView.SaveGraphToDisk();
        }

        protected virtual void Update() {
            if (reloadWorkaround && graph != null) {
                LoadGraph();
                reloadWorkaround = false;
            }
        }

        protected virtual void OnDestroy() { }

        private void InitializeRootView() {
            root = rootVisualElement; // Each editor window contains a root VisualElement object

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            root.styleSheets.Add(styleSheet);

            twoPaneCustomControl = root.Q<TwoPaneCustomControl>();
            scrollViewCustomControl = root.Q<ScrollViewCustomControl>();
            graphView = root.Q<WGGraphView>();
        }

        private void LoadGraph() {
            if (graph != null) {
                if (graph.isEnabled) // Sanity Check: make sure graph is enabled then initialize graph again
                    InitializeGraph(graph);
                else
                    graph.onEnabled += () => InitializeGraph(graph);
            }
            else {
                reloadWorkaround = true;
            }
        }

        public void InitializeGraph(WorldGraph graph) {
            if (this.graph != null && graph != this.graph) {
                EditorUtility.SetDirty(this.graph); // Save the graph to the disk
                AssetDatabase.SaveAssets();
                graphUnloaded?.Invoke(this.graph); // Unload the graph
            }

            graphLoaded?.Invoke(graph);
            this.graph = graph;

            InitializeWindow(graph);

            if (graphView == null) {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph);
            InitializeGraphView(graphView);
        }

        // private void OnSelectionChange() {
        //     // May not need to do this, if only one world graph per project
        //     var m_worldGraphAsset = Selection.activeObject as WorldGraph;
        //     if (m_worldGraphAsset != null && m_worldGraphAsset != graph) {
        //         graph = m_worldGraphAsset;
        //         Debug.Log(graph.name);
        //     }
        // }

        public virtual void OnGraphDeleted() {
            if (graph != null && graphView != null)
                root.Remove(graphView);

            graphView = null;
        }

        protected abstract void InitializeWindow(WorldGraph graph);
        protected virtual void InitializeGraphView(WGGraphView view) { }
    }
}