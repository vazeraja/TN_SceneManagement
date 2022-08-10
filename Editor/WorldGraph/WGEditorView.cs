using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {
    [Serializable]
    public class UserViewSettings {
        public bool isBlackboardVisible = true;
        public bool isPreviewVisible = true;
        public bool isInspectorVisible = true;
    }

    public class WGEditorView : VisualElement, IDisposable {
        private EditorWindow m_EditorWindow;
        private WGGraphViewX m_GraphViewX;
        private WGGraphView m_GraphView;
        private TwoPaneSplitView m_TwoPaneSplitView;
        private VisualElement m_SplitViewLeftPanel;
        private VisualElement m_SplitViewRightPanel;
        private WorldGraph m_Graph;
        private string m_AssetName;

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

        public WGGraphViewX graphViewX {
            get => m_GraphViewX;
            set => m_GraphViewX = value;
        }
        public WGGraphView graphView {
            get => m_GraphView;
            set {
                if (m_GraphView != null) {
                    m_GraphView.RemoveFromHierarchy();
                    m_GraphView.Dispose();
                }

                m_GraphView = value;

                // ReSharper disable once InvertIf
                if (m_GraphView != null) {
                    graphView.SetupZoom(0.05f, 8);
                    graphView.AddManipulator(new ContentDragger());
                    graphView.AddManipulator(new SelectionDragger());
                    graphView.AddManipulator(new RectangleSelector());
                    graphView.AddManipulator(new ClickSelector());
                    m_TwoPaneSplitView.Q<VisualElement>("left-panel").Add(graphView);
                }
            }
        }

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
                    //saveAsRequested();
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

            m_TwoPaneSplitView = new TwoPaneSplitView(0, 928, TwoPaneSplitViewOrientation.Horizontal) {
                name = "TwoPaneSplitView"
            };
            {
                m_TwoPaneSplitView.Add(new VisualElement {name = "left-panel"});
                {
                    graphView = new WGGraphView(editorWindow) {
                        name = "GraphView", viewDataKey = "MaterialGraphView"
                    };
                }
                m_TwoPaneSplitView.Add(new VisualElement {name = "right-panel"});
            }
            // m_SearchWindowProvider = ScriptableObject.CreateInstance<WGSearcherProvider>();
            // m_SearchWindowProvider.Initialize(m_EditorWindow, m_GraphViewX);
            // m_GraphView.nodeCreationRequest = context => {
            //     var displayPosition = (context.screenMousePosition - m_EditorWindow.position.position);
            //     //only display the search window when current graph view is focused
            //     if (EditorWindow.focusedWindow == m_EditorWindow) 
            //     {
            //         searcherWindow = SearcherWindow.ShowAndGet(m_EditorWindow,
            //             (m_SearchWindowProvider as WGSearcherProvider).LoadSearchWindow(),
            //             item => (m_SearchWindowProvider as WGSearcherProvider).OnSearcherSelectEntry(item,
            //                 context.screenMousePosition - m_EditorWindow.position.position),
            //             displayPosition, null,
            //             new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Center,
            //                 SearcherWindow.Alignment.Horizontal.Left));
            //     }
            // };

            Add(m_TwoPaneSplitView);
        }

        public void Dispose() {
            Debug.Log("Disposing Editor View");

            if (graphView != null) {
                saveRequested = null;
                saveAsRequested = null;
                showInProjectRequested = null;
                isCheckedOut = null;
                checkOut = null;
                // // Get all nodes and remove them from the graphView
                // foreach (var node in m_GraphView.Children().OfType<IShaderNodeView>())
                //     node.Dispose();

                // if (searcherWindow != null) {
                //     searcherWindow.Close();
                //     searcherWindow = null;
                // }
                // 
                // if (m_SearchWindowProvider != null) {
                //     Debug.Log("Window is closed but search provider is not.");
                //     Object.DestroyImmediate(m_SearchWindowProvider);
                //     m_SearchWindowProvider = null;
                // }

                graphView.nodeCreationRequest = null;
                graphView = null;
            }
        }
    }
}