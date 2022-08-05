using ThunderNut.SceneManagement;
using ThunderNut.SceneManagement.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Linq;

namespace ThunderNut.SceneManagement.Editor {
    [Serializable]
    public abstract class WorldGraph_EditorWindow : EditorWindow {
        private const string visualTreePath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uxml";
        private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

        [NonSerialized]
        private bool m_Initialized;

        protected VisualElement root;
        protected WorldGraph graph;
        protected WorldGraph_GraphView graphView;
        protected TwoPaneCustomControl twoPaneCustomControl;
        protected ScrollViewCustomControl scrollViewCustomControl;

        public bool isGraphLoaded => graphView != null && graphView.graph != null;
        private bool reloadWorkaround = false;

        public event Action<WorldGraph> graphLoaded;
        public event Action<WorldGraph> graphUnloaded;
        
        #region Static Methods

        // [MenuItem("World Graph/World Graph")]
        // public static WorldGraph_EditorWindow ShowWindow() {
        //     WorldGraph_EditorWindow window = GetWindow<WorldGraph_EditorWindow>();
        //     window.titleContent = new GUIContent("WorldGraphEditor");
        //
        //     var position = window.position;
        //     position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        //     window.position = position;
        //     window.minSize = new Vector2(1200, 600);
        //
        //     window.Focus();
        //     window.Repaint();
        //     return window;
        // }
        //
        // [OnOpenAsset]
        // public static bool OnOpenAsset(int instanceID, int line) {
        //     var worldGraphAsset = EditorUtility.InstanceIDToObject(instanceID) as WorldGraph;
        //     if (worldGraphAsset == null) return false;
        //
        //     var window = ShowWindow();
        //     window.graph = worldGraphAsset;
        //     window.m_Initialized = false;
        //     return true;
        // }

        #endregion

        // OnGUI - Runs frequently - whenever clicks are registered within the editor window and when repaints happen
        // CreateGUI - Runs once - when the editor window is opened
        protected virtual void CreateGUI() {
            InitializeRootView();

            if (graph != null)
                LoadGraph();
            else
                reloadWorkaround = true;

            twoPaneCustomControl = root.Q<TwoPaneCustomControl>();
            scrollViewCustomControl = root.Q<ScrollViewCustomControl>();
            graphView = root.Q<WorldGraph_GraphView>();

            // scrollViewCustomControl.CreateSceneGUI();
            // scrollViewCustomControl.Q<VisualElement>("TestVE").Add(new ResizableElement());
            // var myList = WorldGraphUtility.FindAssetsByType<SceneHandle>();
            // foreach (var handle in myList) {
            //     Debug.Log(handle.name);
            // }
        }

        protected virtual void OnDisable() {
            if (graph != null && graphView != null)
                graphView.SaveGraphToDisk();
        }

        protected virtual void Update() {
            if (!reloadWorkaround || graph == null) return;
            LoadGraph();
            reloadWorkaround = false;
        }

        protected virtual void OnDestroy() { }

        void LoadGraph() {
            // We wait for the graph to be initialized
            if (graph.isEnabled)
                InitializeGraph(graph);
            else
                graph.onEnabled += () => InitializeGraph(graph);
        }

        private void InitializeRootView() {
            // Each editor window contains a root VisualElement object
            root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            root.styleSheets.Add(styleSheet);
        }

        public void InitializeGraph(WorldGraph graph) {
            if (this.graph != null && graph != this.graph) {
                // Save the graph to the disk
                EditorUtility.SetDirty(this.graph);
                AssetDatabase.SaveAssets();
                // Unload the graph
                graphUnloaded?.Invoke(this.graph);
            }

            graphLoaded?.Invoke(graph);
            this.graph = graph;

            if (graphView != null)
                root.Remove(graphView);

            //Initialize will provide the BaseGraphView
            InitializeWindow(graph);

            graphView = root.Children().FirstOrDefault(e => e is WorldGraph_GraphView) as WorldGraph_GraphView;

            if (graphView == null) {
                Debug.LogError("GraphView has not been added to the BaseGraph root view !");
                return;
            }

            graphView.Initialize(graph);

            InitializeGraphView(graphView);
        }

        private void OnSelectionChange() {
            // May not need to do this, if only one world graph per project
            var m_worldGraphAsset = Selection.activeObject as WorldGraph;
            if (m_worldGraphAsset != null && m_worldGraphAsset != graph) {
                graph = m_worldGraphAsset;
                Debug.Log(graph.name);
            }
        }

        public virtual void OnGraphDeleted() {
            if (graph != null && graphView != null)
                root.Remove(graphView);

            graphView = null;
        }

        protected abstract void InitializeWindow(WorldGraph graph);
        protected virtual void InitializeGraphView(WorldGraph_GraphView view) { }
    }
}