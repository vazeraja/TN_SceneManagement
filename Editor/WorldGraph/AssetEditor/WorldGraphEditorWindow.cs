using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.IO;
using System.Linq;
using Unity.Profiling;
using UnityEditor.Callbacks;
using UnityEditor.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEditorWindow : EditorWindow {
        [NonSerialized] bool m_FrameAllAfterLayout;
        [NonSerialized] bool m_HasError;
        [NonSerialized] bool m_ProTheme;

        [SerializeField] private string m_Selected;
        private string selectedGuid {
            get => m_Selected;
            set => m_Selected = value;
        }

        private WorldGraph m_WorldGraph;
        public WorldGraph worldGraph {
            get => m_WorldGraph;
            set => m_WorldGraph = value;
        }

        private WorldGraphEditorView m_GraphEditorView;
        private WorldGraphEditorView graphEditorView {
            get => m_GraphEditorView;
            set {
                if (m_GraphEditorView != null) {
                    m_GraphEditorView.RemoveFromHierarchy();
                    m_GraphEditorView.Dispose();
                }

                m_GraphEditorView = value;

                // ReSharper disable once InvertIf
                if (m_GraphEditorView != null) {
                    m_GraphEditorView.toolbar.saveRequested += () => { };
                    m_GraphEditorView.toolbar.saveAsRequested += () => { };
                    m_GraphEditorView.toolbar.showInProjectRequested += PingAsset;
                    m_GraphEditorView.toolbar.refreshRequested += Refresh;
                    // m_GraphEditorView.toolbar.isCheckedOut += IsGraphAssetCheckedOut;
                    // m_GraphEditorView.toolbar.checkOut += CheckoutAsset;
                    m_GraphEditorView.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
                    m_FrameAllAfterLayout = true;
                    rootVisualElement.Add(graphEditorView);
                }
            }
        }

        private static readonly ProfilerMarker GraphLoadMarker = new ProfilerMarker("GraphLoad");
        private static readonly ProfilerMarker CreateGraphEditorViewMarker = new ProfilerMarker("CreateGraphEditorView");

        public static bool ShowWorldGraphEditorWindow(string path) {
            string guid = AssetDatabase.AssetPathToGUID(path);

            foreach (var w in Resources.FindObjectsOfTypeAll<WorldGraphEditorWindow>()) {
                if (w.selectedGuid != guid) continue;
                w.Focus();
                return true;
            }

            var window = CreateWindow<WorldGraphEditorWindow>(typeof(WorldGraphEditorWindow), typeof(SceneView));
            window.minSize = new Vector2(1200, 600);
            window.Initialize(guid);
            window.Focus();

            return true;
        }

        [OnOpenAsset(0)]
        public static bool OnBaseGraphOpened(int instanceID, int line) {
            var asset = EditorUtility.InstanceIDToObject(instanceID) as WorldGraph;
            string path = AssetDatabase.GetAssetPath(instanceID);

            if (asset == null || !path.Contains("WorldGraph"))
                return false;

            return ShowWorldGraphEditorWindow(path);
        }

        protected void OnEnable() {
            this.SetAntiAliasing(4);
        }

        protected void Update() {
            if (m_HasError) return;

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

                    graphEditorView = new WorldGraphEditorView(this, asset, graphName) {
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

        protected void OnDisable() {
            graphEditorView = null;
        }

        public void Initialize(WorldGraphEditorWindow other) { }

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
                    graphEditorView = new WorldGraphEditorView(this, worldGraph, graphName) {
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

            string newTitle = graphName;
            if (worldGraph == null)
                newTitle += " (nothing loaded)";
            else {
                if (!AssetFileExists())
                    newTitle += " (deleted)";
            }

            Texture2D icon;
            {
                icon = Resources.Load<Texture2D>("Sprite-0002");
            }
            titleContent = new GUIContent(newTitle, icon);
        }

        private void SaveAs() {
            SaveAsImplementation(true);
        }

        // ReSharper disable once UnusedMethodReturnValue.Local
        private string SaveAsImplementation(bool openWhenSaved) {
            string savedFilePath = "";
            if (openWhenSaved) {
                Debug.Log("TODO: Implement SaveAsImplementation()");
                return savedFilePath;
            }
            else {
                return "TODO: Implement SaveAsImplementation()";
            }
        }

        private void PingAsset() {
            string path = AssetDatabase.GUIDToAssetPath(selectedGuid);
            if (selectedGuid == null) return;
            var asset = AssetDatabase.LoadAssetAtPath<Object>(path);
            EditorGUIUtility.PingObject(asset);
            Selection.activeObject = asset;
        }

        public void Refresh() {
            OnDisable();
            OnEnable();
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