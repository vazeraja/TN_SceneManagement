using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    [Serializable]
    public class UserViewSettings {
        public bool isBlackboardVisible = true;
        public bool isInspectorVisible = true;
        public bool isPreviewVisible = true;
    }

    public class WGEditorView : VisualElement, IDisposable {
        private EditorWindow m_EditorWindow;
        private WGGraphView m_GraphView;

        private WorldGraph m_Graph;
        private WorldGraphEditor graphEditor;
        private string m_AssetName;

        private Vector2 scrollPos;

        private Blackboard exposedPropertiesBlackboard;
        private Blackboard inspectorBlackboard;
        private MasterPreviewView masterPreviewView;
        private GenericMenu exposedPropertiesItemMenu;

        private BaseEdgeConnectorListener connectorListener;
        private WGSearcherProvider m_SearchWindowProvider;
        private SearcherWindow searcherWindow;

        public Action saveRequested { get; set; }
        public Action saveAsRequested { get; set; }
        public Action showInProjectRequested { get; set; }
        public Action refreshRequested { get; set; }
        public Func<bool> isCheckedOut { get; set; }
        public Action checkOut { get; set; }

        const string k_UserViewSettings = "TN.WorldGraph.ToggleSettings";
        private readonly UserViewSettings m_UserViewSettings;


        const string k_PreviewWindowLayoutKey = "TN.WorldGraph.PreviewWindowLayout";

        private WindowDockingLayout previewDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
        };

        private const string k_InspectorWindowLayoutKey = "TN.WorldGraph.InspectorWindowLayout";

        private WindowDockingLayout inspectorDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        const string k_PropertiesWindowLayoutKey = "TN.WorldGraph.ExposedPropertiesWindowLayout";

        private WindowDockingLayout exposedPropertiesDockingLayout { get; set; } = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = true,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

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

            graphEditor = UnityEditor.Editor.CreateEditor(graph) as WorldGraphEditor;

            string serializedSettings = EditorUserSettings.GetConfigValue(k_UserViewSettings);
            m_UserViewSettings = JsonUtility.FromJson<UserViewSettings>(serializedSettings) ?? new UserViewSettings();

            var toolbar = new IMGUIContainer(() => {
                GUILayout.BeginHorizontal(EditorStyles.toolbar);
                {
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

                    GUILayout.Space(6);
                    if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
                        refreshRequested?.Invoke();
                    }

                    GUILayout.FlexibleSpace();

                    EditorGUI.BeginChangeCheck();
                    m_UserViewSettings.isBlackboardVisible =
                        GUILayout.Toggle(m_UserViewSettings.isBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);

                    GUILayout.Space(6);

                    m_UserViewSettings.isInspectorVisible =
                        GUILayout.Toggle(m_UserViewSettings.isInspectorVisible, "Graph Inspector", EditorStyles.toolbarButton);

                    GUILayout.Space(6);

                    m_UserViewSettings.isPreviewVisible =
                        GUILayout.Toggle(m_UserViewSettings.isPreviewVisible, "Main Preview", EditorStyles.toolbarButton);

                    if (EditorGUI.EndChangeCheck()) {
                        UserViewSettingsChangeCheck();
                    }
                }
                GUILayout.EndHorizontal();
            });
            Add(toolbar);

            var content = new VisualElement {name = "content"};
            {
                m_GraphView = new WGGraphView(graph) {
                    name = "GraphView", viewDataKey = "MaterialGraphView"
                };
                m_GraphView.styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
                m_GraphView.AddManipulator(new ContentDragger());
                m_GraphView.AddManipulator(new SelectionDragger());
                m_GraphView.AddManipulator(new RectangleSelector());
                m_GraphView.AddManipulator(new ClickSelector());
                m_GraphView.SetupZoom(0.05f, 8);

                content.Add(m_GraphView);

                string serializedPreview = EditorUserSettings.GetConfigValue(k_PreviewWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedPreview)) {
                    previewDockingLayout = JsonUtility.FromJson<WindowDockingLayout>(serializedPreview) ?? new WindowDockingLayout();
                }

                string serializedInspector = EditorUserSettings.GetConfigValue(k_InspectorWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedInspector)) {
                    inspectorDockingLayout =
                        JsonUtility.FromJson<WindowDockingLayout>(serializedInspector) ?? new WindowDockingLayout();
                }

                string serializedBlackboard = EditorUserSettings.GetConfigValue(k_PropertiesWindowLayoutKey);
                if (!string.IsNullOrEmpty(serializedBlackboard)) {
                    exposedPropertiesDockingLayout =
                        JsonUtility.FromJson<WindowDockingLayout>(serializedBlackboard) ?? new WindowDockingLayout();
                }

                CreateMasterPreview();
                CreateInspectorBlackboard();
                CreateExposedPropertiesBlackboard();

                UpdateSubWindowsVisibility();

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WGSearcherProvider>();
            m_SearchWindowProvider.Initialize(editorWindow, m_GraphView, SearchWindowItemSelected);
            m_GraphView.nodeCreationRequest = c => {
                if (EditorWindow.focusedWindow != m_EditorWindow) return;
                var displayPosition = (c.screenMousePosition - m_EditorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(m_EditorWindow, m_SearchWindowProvider.LoadSearchWindow(),
                    item => m_SearchWindowProvider.OnSearcherSelectEntry(item, displayPosition), displayPosition, null);
            };

            Add(content);
        }
        
        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView(m_Graph) {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            m_GraphView.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void CreateInspectorBlackboard() {
            inspectorBlackboard = new Blackboard(m_GraphView) {title = "Inspector", subTitle = "WorldGraph"};
            {
                inspectorBlackboard.Add(new IMGUIContainer(() => {
                    // ReSharper disable once ConvertToUsingDeclaration
                    using (var scrollViewScope = new EditorGUILayout.ScrollViewScope(scrollPos)) {
                        scrollPos = scrollViewScope.scrollPosition;
                        //graphEditor!.OnInspectorGUI();
                    }
                }));
            }
            m_GraphView.Add(inspectorBlackboard);
        }

        private void CreateExposedPropertiesBlackboard() {
            exposedPropertiesBlackboard = new Blackboard(m_GraphView) {title = "Exposed Properties", subTitle = "WorldGraph"};
            {
                exposedPropertiesItemMenu = new GenericMenu();
                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => Debug.Log("String"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => Debug.Log("Float"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => Debug.Log("Int"));
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => Debug.Log("Bool"));
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedPropertiesBlackboard.addItemRequested += b => exposedPropertiesItemMenu.ShowAsContext();
            }

            m_GraphView.Add(exposedPropertiesBlackboard);
        }
        
        private void ApplySerializedWindowLayouts(GeometryChangedEvent evt) {
            UnregisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            ApplySerializedLayout(inspectorBlackboard, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            ApplySerializedLayout(exposedPropertiesBlackboard, exposedPropertiesDockingLayout, k_PropertiesWindowLayoutKey);
            ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
        }
        
        private void ApplySerializedLayout(VisualElement target, WindowDockingLayout layout, string layoutKey) {
            layout.ApplySize(target);
            layout.ApplyPosition(target);

            target.RegisterCallback<GeometryChangedEvent>((evt) => {
                layout.CalculateDockingCornerAndOffset(target.layout, m_GraphView.layout);
                layout.ClampToParentWindow();

                string serializedWindowLayout = JsonUtility.ToJson(layout);
                EditorUserSettings.SetConfigValue(layoutKey, serializedWindowLayout);
            });
        }

        private void SearchWindowItemSelected(Type type) {
            SceneHandle newHandle = m_Graph.CreateSubAsset(type);
            var node = new WGNodeView(m_GraphView, newHandle);

            m_GraphView.AddElement(node);
            Debug.Log("Node Count: " + m_GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());
        }

        private void UserViewSettingsChangeCheck() {
            string serializedUserViewSettings = JsonUtility.ToJson(m_UserViewSettings);
            EditorUserSettings.SetConfigValue(k_UserViewSettings, serializedUserViewSettings);

            UpdateSubWindowsVisibility();
        }

        private void UpdateSubWindowsVisibility() {
            exposedPropertiesBlackboard.visible = m_UserViewSettings.isBlackboardVisible;
            inspectorBlackboard.visible = m_UserViewSettings.isInspectorVisible;
            masterPreviewView.visible = m_UserViewSettings.isPreviewVisible;
        }

        public void Dispose() {
            if (m_GraphView != null) {
                saveRequested = null;
                saveAsRequested = null;
                showInProjectRequested = null;
                isCheckedOut = null;
                checkOut = null;

                // Get all nodes and dispose
                // Debug.Log("Node Count: " + m_GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());
                foreach (IWorldGraphNodeView node in m_GraphView.graphElements.OfType<IWorldGraphNodeView>()) {
                    m_Graph.RemoveSubAsset(node.sceneHandle);
                    m_GraphView.RemoveElement(node.gvNode);

                    node.Dispose();
                }
                // Debug.Log("Node Count: " + m_GraphView.graphElements.OfType<IWorldGraphNodeView>().Count());

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