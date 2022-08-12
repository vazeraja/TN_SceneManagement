using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {
    [Serializable]
    internal class FloatingWindowsLayout
    {
        public WindowDockingLayout previewLayout = new WindowDockingLayout
        {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8
        };
    }
    [Serializable]
    internal class UserViewSettings {
        public bool isBlackboardVisible = true;
        public bool isPreviewVisible = true;
        public bool isInspectorVisible = true;
    }

    internal class WGEditorView : VisualElement, IDisposable {
        private EditorWindow m_EditorWindow;
        private WGGraphView m_GraphView;
        private WorldGraph m_Graph;

        private string m_AssetName;

        private TwoPaneSplitView m_TwoPaneSplitView;
        private int m_FPIndex = 0;
        private float m_FPInitialDimension = 928;
        private TwoPaneSplitViewOrientation splitViewOrientation = TwoPaneSplitViewOrientation.Horizontal;

        private BaseEdgeConnectorListener connectorListener;
        private SearchWindowProvider m_SearchWindowProvider;
        private SearcherWindow searcherWindow;
        private UserViewSettings m_UserViewSettings;

        public Action saveRequested { get; set; }
        public Action saveAsRequested { get; set; }
        public Action showInProjectRequested { get; set; }
        public Func<bool> isCheckedOut { get; set; }
        public Action checkOut { get; set; }

        const string k_UserViewSettings = "UnityEditor.ShaderGraph.ToggleSettings";
        public UserViewSettings viewSettings => m_UserViewSettings;
        
        const string k_FloatingWindowsLayoutKey = "UnityEditor.ShaderGraph.FloatingWindowsLayout2";
        FloatingWindowsLayout m_FloatingWindowsLayout = new FloatingWindowsLayout();

        public WGGraphView graphView => m_GraphView;

        public string assetName {
            get => m_AssetName;
            set {
                m_AssetName = value;
                // Also update blackboard title
                // m_BlackboardController.UpdateBlackboardTitle(m_AssetName);
            }
        }

        public WGEditorView(EditorWindow editorWindow, WorldGraph graph, string graphName) {
            m_EditorWindow = editorWindow;
            m_Graph = graph;
            m_AssetName = graphName;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEditorView"));
            var serializedSettings = EditorUserSettings.GetConfigValue(k_UserViewSettings);
            m_UserViewSettings = JsonUtility.FromJson<UserViewSettings>(serializedSettings) ?? new UserViewSettings();

            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton)) {
                    //saveRequested?.Invoke();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton)) {
                    saveAsRequested();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    showInProjectRequested?.Invoke();
                }
                
                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();

                m_UserViewSettings.isBlackboardVisible = GUILayout.Toggle(m_UserViewSettings.isBlackboardVisible,
                    "Blackboard", EditorStyles.toolbarButton);

                GUILayout.Space(6);

                m_UserViewSettings.isInspectorVisible = GUILayout.Toggle(m_UserViewSettings.isInspectorVisible,
                    "Graph Inspector", EditorStyles.toolbarButton);

                GUILayout.Space(6);

                m_UserViewSettings.isPreviewVisible = GUILayout.Toggle(m_UserViewSettings.isPreviewVisible,
                    "Main Preview", EditorStyles.toolbarButton);
                if (EditorGUI.EndChangeCheck()) {
                    // Do something here such as showing/hiding those windows
                    Debug.Log("Do something here such as showing/hiding windows");
                }

                GUILayout.EndHorizontal();
            });
            Add(toolbar);

            m_TwoPaneSplitView = new TwoPaneSplitView(m_FPIndex, m_FPInitialDimension, splitViewOrientation) {
                name = "TwoPaneSplitView"
            };
            {
                m_TwoPaneSplitView.Add(new VisualElement {name = "left-panel"});
                {
                    m_GraphView = new WGGraphView(graph) {
                        name = "GraphView", viewDataKey = "MaterialGraphView"
                    };
                    m_GraphView.SetupZoom(0.05f, 8);
                    m_GraphView.AddManipulator(new ContentDragger());
                    m_GraphView.AddManipulator(new SelectionDragger());
                    m_GraphView.AddManipulator(new RectangleSelector());
                    m_GraphView.AddManipulator(new ClickSelector());
                    
                    string serializedWindowLayout = EditorUserSettings.GetConfigValue(k_FloatingWindowsLayoutKey);
                    
                    
                    m_TwoPaneSplitView.Q<VisualElement>("left-panel").Add(m_GraphView);
                }
                m_TwoPaneSplitView.Add(new VisualElement {name = "right-panel"});
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WGSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, m_GraphView);
            m_GraphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != m_EditorWindow) return;
                var displayPosition = (c.screenMousePosition - m_EditorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(m_EditorWindow,
                    ((WGSearcherProvider) m_SearchWindowProvider).LoadSearchWindow(),
                    item => ((WGSearcherProvider) m_SearchWindowProvider).OnSearcherSelectEntry(item,
                        c.screenMousePosition - m_EditorWindow.position.position),
                    displayPosition, null);
            };

            Add(m_TwoPaneSplitView);
        }

        public void Dispose() {
            if (m_GraphView != null) {
                saveRequested = null;
                saveAsRequested = null;
                showInProjectRequested = null;
                isCheckedOut = null;
                checkOut = null;
                // // Get all nodes and remove them from the graphView
                // foreach (var node in m_GraphView.Children().OfType<IShaderNodeView>())
                //     node.Dispose();

                m_GraphView.nodeCreationRequest = null;
                m_GraphView = null;
            }

            if (m_SearchWindowProvider != null) {
                Object.DestroyImmediate(m_SearchWindowProvider);
                m_SearchWindowProvider = null;
            }
        }
    }
}