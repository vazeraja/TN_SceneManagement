using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraphEditor : EditorWindow {
        [MenuItem("Window/UI Toolkit/WorldGraphEditor")]
        public static void ShowExample() {
            WorldGraphEditor wnd = GetWindow<WorldGraphEditor>();
            wnd.titleContent = new GUIContent("WorldGraphEditor");
        }

        public void CreateGUI() {
            // Each editor window contains a root VisualElement object
            VisualElement root = rootVisualElement;

            // VisualElements objects can contain other VisualElement following a tree hierarchy.
            VisualElement label = new Label("Hello World! From C#");
            root.Add(label);

            // Import UXML
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditor.uxml");
            VisualElement labelFromUXML = visualTree.Instantiate();
            root.Add(labelFromUXML);

            // A stylesheet can be added to a VisualElement.
            // The style will be applied to the VisualElement and all of its children.
            var styleSheet =
                AssetDatabase.LoadAssetAtPath<StyleSheet>(
                    "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditor.uss");
            VisualElement labelWithStyle = new Label("Hello World! With Style");
            labelWithStyle.styleSheets.Add(styleSheet);
            root.Add(labelWithStyle);
        }
    }
}