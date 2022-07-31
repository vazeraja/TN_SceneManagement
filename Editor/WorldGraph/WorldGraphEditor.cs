using ThunderNut.SceneManagement;
using ThunderNut.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

public class WorldGraphEditor : EditorWindow {
    [MenuItem("World Graph/World Graph")]
    public static void ShowWindow() {
        WorldGraphEditor wnd = GetWindow<WorldGraphEditor>();
        wnd.titleContent = new GUIContent("WorldGraphEditor");
    }
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceId, int line)
    {
        if (!(Selection.activeObject is WorldGraph)) return false;
        ShowWindow();
        return true;
    }

    private const string visualTreePath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditor.uxml";
    private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditor.uss";

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
        
        

        //scrollView = root.Q<ScrollViewCustomControl>();
        //scrollView.CreateSceneGUI();
    }
}