using ThunderNut.SceneManagement;
using ThunderNut.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldGraph_EditorWindow : EditorWindow {
    [MenuItem("World Graph/World Graph")]
    public static WorldGraph_EditorWindow ShowWindow() {
        WorldGraph_EditorWindow window = GetWindow<WorldGraph_EditorWindow>();
        window.titleContent = new GUIContent("WorldGraphEditor");
        
        var position = window.position;
        position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;
        
        window.Focus();
        window.Repaint();
        return window;
    }
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (!(Selection.activeObject is WorldGraph)) return false;
        ShowWindow();
        return true;
    }

    private const string visualTreePath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uxml";
    private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

    private TwoPaneCustomControl twoPaneCustomControl;

    public void CreateGUI() {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
        root.styleSheets.Add(styleSheet);
        
        twoPaneCustomControl = root.Q<TwoPaneCustomControl>();
        var rightPanel = twoPaneCustomControl.Q<VisualElement>("right-panel");
        var scrollView = new SceneSelectorCustomControl();
        rightPanel.Add(scrollView);
        scrollView.CreateSceneGUI();
        
    }
}