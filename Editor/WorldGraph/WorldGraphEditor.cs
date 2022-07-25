using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraphEditor : EditorWindow {
        
        [MenuItem("World Graph/World Graph")]
        public static void ShowWindow() {
            WorldGraphEditor wnd = GetWindow<WorldGraphEditor>();
            wnd.minSize = new Vector2(100, 150);
            wnd.titleContent = new GUIContent("WorldGraph");
            wnd.Show();
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

        private ScrollViewCustomControl scrollView;

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(visualTreePath);
            visualTree.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            root.styleSheets.Add(styleSheet);

            scrollView = root.Q<ScrollViewCustomControl>();
            scrollView.CreateSceneGUI();
        }
    }
}