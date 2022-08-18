using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using Unity.Profiling;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {
    public class WGEditorWindow : EditorWindow {
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
        
        public static bool ShowWorldGraphEditorWindow(string path) {
            string guid = AssetDatabase.AssetPathToGUID(path);
            
            foreach (var w in Resources.FindObjectsOfTypeAll<WGEditorWindow>()) {
                if (w.selectedGuid != guid) continue;
                w.Focus();
                return true;
            }
            
            var window = EditorWindow.CreateWindow<WGEditorWindow>(typeof(WGEditorWindow), typeof(SceneView));
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

        protected virtual void OnEnable() {
            this.SetAntiAliasing(4);
        }

        protected virtual void Update() {
            if (m_HasError)
                return;
            var updateTitle = false;

            try {
                if (worldGraph == null && selectedGuid != null) {
                    string guid = selectedGuid;
                    selectedGuid = null;
                    Initialize(guid);
                }

                if (worldGraph == null) {
                    Close();
                    return;
                }

                if (graphEditorView == null) {
                    string assetPath = AssetDatabase.GUIDToAssetPath(selectedGuid);

                    string graphName = Path.GetFileNameWithoutExtension(assetPath);
                    var asset = AssetDatabase.LoadAssetAtPath<WorldGraph>(assetPath);

                    graphEditorView = new WGEditorView(this, asset, graphName) {
                        viewDataKey = selectedGuid,
                    };

                    updateTitle = true;
                }

                if (updateTitle)
                    UpdateTitle();
            }
            catch (Exception e) {
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

        public void Initialize(WGEditorWindow other) { }

        public void Initialize(string assetGuid) {
            try {
                WorldGraph asset = AssetDatabase.LoadAssetAtPath<WorldGraph>(AssetDatabase.GUIDToAssetPath(assetGuid));

                if (asset == null || !EditorUtility.IsPersistent(asset) || selectedGuid == assetGuid)
                    return;

                string path = AssetDatabase.GetAssetPath(asset);

                selectedGuid = assetGuid;
                string graphName = Path.GetFileNameWithoutExtension(path);

                using (GraphLoadMarker.Auto()) {
                    worldGraph = asset;
                }

                using (CreateGraphEditorViewMarker.Auto()) {
                    graphEditorView = new WGEditorView(this, worldGraph, graphName) {
                        viewDataKey = assetGuid,
                    };
                }

                UpdateTitle();

                Repaint();
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

        public void SaveAs() {
            SaveAsImplementation(true);
        }

        public string SaveAsImplementation(bool openWhenSaved) {
            string savedFilePath = "";
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
                Selection.activeObject = asset;
            }
        }

        private bool AssetFileExists() => File.Exists(AssetDatabase.GUIDToAssetPath(selectedGuid));

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
    }
}