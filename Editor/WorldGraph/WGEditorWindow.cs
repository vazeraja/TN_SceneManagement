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
        [SerializeField] private string m_Selected;
        [SerializeField] bool m_AssetMaybeChangedOnDisk;
        [SerializeField] bool m_AssetMaybeDeleted;
        [NonSerialized] bool m_FrameAllAfterLayout;
        [NonSerialized] bool m_HasError;
        [NonSerialized] bool m_ProTheme;
        [SerializeField] string m_LastSerializedFileContents;

        public string selectedGuid {
            get => m_Selected;
            private set => m_Selected = value;
        }

        [SerializeField] protected WorldGraph m_WorldGraph;
        public WorldGraph worldGraph {
            get => m_WorldGraph;
            set { m_WorldGraph = value; }
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
                    m_GraphEditorView.saveAsRequested += SaveAs;
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

        protected virtual void Update() {
            if (m_HasError)
                return;
            
            bool updateTitle = false;

            if (m_AssetMaybeDeleted) {
                m_AssetMaybeDeleted = false;
                if (AssetFileExists()) {
                    m_AssetMaybeChangedOnDisk = true;
                }
                else {
                    DisplayDeletedFromDiskDialog();
                }

                updateTitle = true;
            }

            if (EditorGUIUtility.isProSkin != m_ProTheme) {
                if (worldGraph != null) {
                    updateTitle = true; // trigger icon swap
                    m_ProTheme = EditorGUIUtility.isProSkin;
                }
            }

            if (m_AssetMaybeChangedOnDisk) {
                m_AssetMaybeChangedOnDisk = false;
                
                // if(worldGraph.graph != null){
                //     // Do stuff here when we are actually serializing graph contents
                //     // Such as checking if the contents serialized contents differ from the disk contents
                // }
                updateTitle = true;
            }

            try {
                if (worldGraph == null && selectedGuid != null) {
                    
                }
            }
            catch (Exception e)
            {
                m_HasError = true;
                m_GraphEditorView = null;
                m_WorldGraph = null;
                Debug.LogException(e);
                throw;
            }
        }

        protected virtual void OnDisable() {
            worldGraph = null;
            graphEditorView = null;
        }

        public void Initialize(string assetGuid) { }

        public void Initialize(WorldGraph graph, string path, string assetGuid) {
            try {
                selectedGuid = assetGuid;
                string graphName = Path.GetFileNameWithoutExtension(path);

                using (GraphLoadMarker.Auto()) {
                    worldGraph = graph;

                    // TODO: Replace with Custom WG Icon instead of SG icons
                    Texture2D icon;
                    {
                        string theme = EditorGUIUtility.isProSkin ? "_dark" : "_light";
                        icon = Resources.Load<Texture2D>("Icons/sg_graph_icon_gray" + theme);
                    }
                    titleContent = new GUIContent(graphName, icon);
                }

                using (CreateGraphEditorViewMarker.Auto()) {
                    graphEditorView = new WGEditorView(this, worldGraph, graphName) {
                        viewDataKey = assetGuid,
                    };
                }
            }
            catch (Exception) {
                m_HasError = true;
                m_WorldGraph = null;
                m_GraphEditorView = null;
                throw;
            }
        }

        public void UpdateTitle() {
            string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);
            string graphName = Path.GetFileNameWithoutExtension(assetPath);

            // update blackboard title (before we add suffixes)
            if (graphEditorView != null)
                graphEditorView.assetName = graphName;

            string title = graphName;
            if (worldGraph == null)
                title += " (nothing loaded)";
            else {
                if (!AssetFileExists())
                    title += " (deleted)";
            }

            // TODO: Replace with Custom WG Icon instead of SG icons
            Texture2D icon;
            {
                string theme = EditorGUIUtility.isProSkin ? "_dark" : "_light";
                icon = Resources.Load<Texture2D>("Icons/sg_graph_icon_gray" + theme);
            }
            titleContent = new GUIContent(title, icon);
        }

        private bool DisplayDeletedFromDiskDialog(bool reopen = true) {
            // first double check if we've actually been deleted
            bool saved = false;
            bool okToClose = false;
            string originalAssetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);

            while (true) {
                int option = EditorUtility.DisplayDialogComplex(
                    "Graph removed from project",
                    "The file has been deleted or removed from the project folder.\n\n" +
                    originalAssetPath +
                    "\n\nWould you like to save your Graph Asset?",
                    "Save As...", "Cancel", "Discard Graph and Close Window");

                if (option == 0) {
                    string savedPath = SaveAsImplementation(false);
                    Debug.Log(savedPath);
                    // if (savedPath != null)
                    // {
                    //     saved = true;
                    // 
                    //     // either close or reopen the local window editor
                    //     worldGraph = null;
                    //     selectedGuid = (reopen ? AssetDatabase.AssetPathToGUID(savedPath) : null);
                    // 
                    //     break;
                    // }
                }
                else if (option == 1) {
                    // continue in deleted state...
                    break;
                }
                else if (option == 2) {
                    okToClose = true;
                    worldGraph = null;
                    selectedGuid = null;
                    break;
                }
            }

            return (saved || okToClose);
        }

        public void SaveAs() {
            SaveAsImplementation(true);
        }

        public string SaveAsImplementation(bool openWhenSaved) {
            string savedFilePath = null;
            if (openWhenSaved) {
                Debug.Log("TODO: Implement SaveAsImplementation()");
                return savedFilePath;
            }
            else {
                return "TODO: Implement SaveAsImplementation()";
            }
        }
        public void PingAsset() {
            if (selectedGuid != null) {
                var path = AssetDatabase.GUIDToAssetPath(selectedGuid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
                EditorGUIUtility.PingObject(asset);
            }
        }

        private bool AssetFileExists() => File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid));

        public void AssetWasDeleted() {
            m_AssetMaybeDeleted = true;
            UpdateTitle();
        }

        public void CheckForChanges() {
            if (!m_AssetMaybeDeleted && worldGraph != null) {
                m_AssetMaybeChangedOnDisk = true;
                UpdateTitle();
            }
        }

        private void OnGeometryChanged(GeometryChangedEvent evt) {
            if (graphEditorView == null)
                return;

            // this callback is only so we can run post-layout behaviors after the graph loads for the first time
            // we immediately unregister it so it doesn't get called again
            graphEditorView.UnregisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            if (m_FrameAllAfterLayout) {
                graphEditorView.graphView.FrameAll();
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

        protected virtual void OnDestroy() { }
        public virtual void OnGraphDeleted() { }
        protected abstract void InitializeWindow(WorldGraph graph);
        protected virtual void InitializeGraphView(WGGraphView view) { }
    }
}