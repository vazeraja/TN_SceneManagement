using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using Unity.Profiling;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {
    [Serializable]
    public abstract class WGEditorWindow : EditorWindow {
        [NonSerialized]
        bool m_FrameAllAfterLayout;

        [SerializeField]
        private string m_Selected;
        public string selectedGuid {
            get => m_Selected;
            private set => m_Selected = value;
        }

        protected WorldGraph m_WorldGraph;
        public WorldGraph worldGraph {
            get => m_WorldGraph;
            set => m_WorldGraph = value;
        }

        protected WGEditorView m_GraphEditorView;
        public WGEditorView graphEditorView {
            get => m_GraphEditorView;
            private set {
                if (m_GraphEditorView != null) {
                    m_GraphEditorView.RemoveFromHierarchy();
                    m_GraphEditorView.Dispose();
                }

                m_GraphEditorView = value;

                // ReSharper disable once InvertIf
                if (m_GraphEditorView != null) {
                    // m_GraphEditorView.saveRequested += () => SaveAsset();
                    // m_GraphEditorView.saveAsRequested += SaveAs;
                    // m_GraphEditorView.convertToSubgraphRequested += ToSubGraph;
                    // m_GraphEditorView.isCheckedOut += IsGraphAssetCheckedOut;
                    // m_GraphEditorView.checkOut += CheckoutAsset;
                    m_GraphEditorView.showInProjectRequested += PingAsset;
                    m_GraphEditorView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;
                    rootVisualElement.Add(graphEditorView);
                }
            }
        }

        private static readonly ProfilerMarker GraphLoadMarker = new("GraphLoad");
        private static readonly ProfilerMarker CreateGraphEditorViewMarker = new("CreateGraphEditorView");

        protected virtual void OnEnable() {
            this.SetAntiAliasing(4);
        }

        protected virtual void OnDisable() {
            worldGraph = null;
            graphEditorView = null;

            // if (graph != null && m_GraphView != null)
            //     m_GraphView.SaveGraphToDisk();
        }

        public void Initialize(WorldGraph graph, string path, string assetGuid) {
            try {
                selectedGuid = assetGuid;
                string graphName = Path.GetFileNameWithoutExtension(path);

                using (GraphLoadMarker.Auto()) {
                    worldGraph = graph;
                }

                using (CreateGraphEditorViewMarker.Auto()) {
                    graphEditorView = new WGEditorView(this, worldGraph, graphName) {
                        viewDataKey = assetGuid,
                    };
                }
            }
            catch (Exception) {
                worldGraph = null;
                graphEditorView = null;
                throw;
            }
        }

        public void PingAsset() {
            if (selectedGuid != null) {
                var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(asset);
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            if (graphEditorView == null)
                return;

            // this callback is only so we can run post-layout behaviors after the graph loads for the first time
            // we immediately unregister it so it doesn't get called again
            graphEditorView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (m_FrameAllAfterLayout) {
                Debug.Log("FrameAll()");
                //graphEditorView.graphView.FrameAll();
            }

            m_FrameAllAfterLayout = false;
        }

        // private const string visualTreePath = "UXML/WorldGraphEditorWindow";
        // private const string styleSheetPath = "Styles/WorldGraphEditorWindow";

        // protected VisualElement root;
        // protected VisualElement leftPanel;
        // protected VisualElement rightPanel;
        // protected WorldGraph graph;
        // protected WGGraphView m_GraphView;
        // 
        // protected TwoPaneCustomControl twoPaneCustomControl;
        // protected ScrollViewCustomControl scrollViewCustomControl;
        // 
        // public bool isGraphLoaded => m_GraphView != null && m_GraphView.graph != null;
        // private bool reloadWorkaround = false;
        // 
        // public event Action<WorldGraph> graphLoaded;
        // public event Action<WorldGraph> graphUnloaded;

        /// <summary>
        /// OnGUI() - Runs frequently - whenever clicks are registered within the editor window and when repaints happen
        /// CreateGUI() - Runs once - when the editor window is opened
        /// CreateGUI() is also very similar to OnEnable()
        /// </summary>
        protected virtual void CreateGUI() {
            // Debug.Log("CreateGUI: OnEnable()");
            // InitializeRootView();
            // LoadGraph();
        }


        protected virtual void Update() {
            // if (reloadWorkaround && graph != null) {
            //     LoadGraph();
            //     reloadWorkaround = false;
            // }
        }

        protected virtual void OnDestroy() { }

        private void InitializeRootView() {
            // root = rootVisualElement; // Each editor window contains a root VisualElement object
            // 
            // var visualTree = Resources.Load<VisualTreeAsset>(visualTreePath);
            // visualTree.CloneTree(root);
            // 
            // var styleSheet = Resources.Load<StyleSheet>(styleSheetPath);
            // root.styleSheets.Add(styleSheet);
            // 
            // twoPaneCustomControl = root.Q<TwoPaneCustomControl>();
            // scrollViewCustomControl = root.Q<ScrollViewCustomControl>();
            // leftPanel = root.Q<VisualElement>("left-panel");
            // rightPanel = root.Q<VisualElement>("right-panel");
        }

        private void LoadGraph() {
            // if (graph != null) {
            //     if (graph.isEnabled) // Sanity Check: make sure graph is enabled then initialize graph again
            //         InitializeGraph(graph);
            //     else
            //         graph.onEnabled += () => InitializeGraph(graph);
            // }
            // else {
            //     reloadWorkaround = true;
            // }
        }

        public void InitializeGraph(WorldGraph graph) {
            // if (this.graph != null && graph != this.graph) {
            //     EditorUtility.SetDirty(this.graph); // Save the graph to the disk
            //     AssetDatabase.SaveAssets();
            //     graphUnloaded?.Invoke(this.graph); // Unload the graph
            // }
            // 
            // graphLoaded?.Invoke(graph);
            // this.graph = graph;
            // 
            // if (m_GraphView != null) 
            //     leftPanel.Remove(m_GraphView);
            // 
            // InitializeWindow(graph);
            // 
            // m_GraphView = leftPanel.Q<WGGraphView>();
            // 
            // if (m_GraphView == null) {
            //     Debug.LogError("GraphView has not been added to the left panel!");
            //     return;
            // }
            // 
            // m_GraphView.Initialize(graph);
            // InitializeGraphView(m_GraphView);
        }

        private void OnSelectionChange() {
            // // May not need to do this, if only one world graph per project
            // var m_worldGraphAsset = Selection.activeObject as WorldGraph;
            // if (m_worldGraphAsset != null && m_worldGraphAsset != graph) {
            //     graph = m_worldGraphAsset;
            //     Debug.Log(graph.name);
            // }
        }

        public virtual void OnGraphDeleted() {
            // if (graph != null && m_GraphView != null)
            //     root.Remove(m_GraphView);
            // 
            // m_GraphView = null;
        }

        protected abstract void InitializeWindow(WorldGraph graph);
        protected virtual void InitializeGraphView(WGGraphView view) { }
    }
}