using ThunderNut.SceneManagement;
using ThunderNut.SceneManagement.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using System;

public class WorldGraph_EditorWindow : EditorWindow {
    private const string visualTreePath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uxml";
    private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

    // [NonSerialized] private bool m_Initialized;
    private WorldGraph worldGraphAsset;
    private TwoPaneCustomControl twoPaneCustomControl;
    private ScrollViewCustomControl scrollViewCustomControl;
    
    [MenuItem("World Graph/World Graph")]
    public static WorldGraph_EditorWindow ShowWindow() {
        WorldGraph_EditorWindow window = GetWindow<WorldGraph_EditorWindow>();
        window.titleContent = new GUIContent("WorldGraphEditor");

        var position = window.position;
        position.center = new Rect(0f, 0f, Screen.currentResolution.width, Screen.currentResolution.height).center;
        window.position = position;
        window.minSize = new Vector2(1200, 600);
        
        window.Focus();
        window.Repaint();
        return window;
    }
    [OnOpenAsset]
    public static bool OnOpenAsset(int instanceID, int line)
    {
        var worldGraphAsset = EditorUtility.InstanceIDToObject (instanceID) as WorldGraph;
        if (worldGraphAsset == null) return false;
        
        var window = ShowWindow();
        window.worldGraphAsset = worldGraphAsset;
        // window.m_Initialized = false;
        return true;
    }

    private void OnSelectionChange() {
        // May not need to do this, if only one world graph per project
        var m_worldGraphAsset = Selection.activeObject as WorldGraph;
        if (m_worldGraphAsset != null && m_worldGraphAsset != worldGraphAsset) {
            worldGraphAsset = m_worldGraphAsset;
            Debug.Log(worldGraphAsset.name);
        }
    }

    // OnGUI - Runs frequently - whenever clicks are registered within the editor window and when repaints happen
    // CreateGUI - Runs once - when the editor window is opened
    public void CreateGUI() {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
        visualTree.CloneTree(root);

        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
        root.styleSheets.Add(styleSheet);
        
        twoPaneCustomControl = root.Q<TwoPaneCustomControl>();
        scrollViewCustomControl = root.Q<ScrollViewCustomControl>();
        
        scrollViewCustomControl.CreateSceneGUI();
        var myList = WorldGraphUtility.FindAssetsByType<SceneHandle>();
        foreach (var handle in myList) {
            Debug.Log(handle.name);
        }
    }
}