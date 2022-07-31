using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {
    class SimpleTreeView : TreeView
    {
        public SimpleTreeView(TreeViewState treeViewState)
            : base(treeViewState)
        {
            Reload();
        }
        
        protected override TreeViewItem BuildRoot ()
        {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we create a fixed set of items. In a real world example,
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = new List<TreeViewItem> 
            {
                new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
                new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
                new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
                new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
                new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
                new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
                new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
                new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
                new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            };
            
            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths (root, allItems);
            
            // Return root of the tree
            return root;
        }
    }
    public class SceneSelectorCustomControl : ScrollView {
        public new class UxmlFactory : UxmlFactory<SceneSelectorCustomControl, ScrollView.UxmlTraits> { }
        
        [InitializeOnLoadMethod]
        private static void RegisterCallbacks()
        {
            EditorApplication.playModeStateChanged += ReturnToPreviousScene;
        }
        private static void ReturnToPreviousScene(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode)
            {
                EditorSceneManager.OpenScene(SceneSelectorSettings.instance.PreviousScenePath, OpenSceneMode.Single);
            }
        }

        public void CreateSceneGUI() {
            string[] sceneGuids = Array.ConvertAll(UnityEditor.EditorBuildSettings.scenes, s => s.guid.ToString());
            foreach (string sceneGuid in sceneGuids) {
                Add(CreateSceneButton(sceneGuid));
            }
        }
        
        Rect buttonRect;
        private IMGUIContainer CreateSceneButton(string sceneGuid) {
            IMGUIContainer container = new IMGUIContainer(() => {
                GUILayout.Label("Editor window with popup", EditorStyles.boldLabel);
                if (GUILayout.Button("Popup", GUILayout.Width(200))) {
                    var simpleTreeView = new SimpleTreeView(new TreeViewState());
                    PopupWindow.Show(buttonRect, new TreeViewPopupWindow(simpleTreeView));
                }
                if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();
            });
            return container;

            // Scene Selector Code
            // string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            // var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            //
            // var buttonGroup = new IMGUIContainer();
            // buttonGroup.style.flexDirection = FlexDirection.Row;
            // buttonGroup.style.marginLeft = 3;
            //
            // var label = new Label($"{sceneAsset.name}");
            // label.style.width = 150;
            // buttonGroup.Add(label);

            // var openButton = new Button(() => {
            //     EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            //     SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
            //     Debug.Log(SceneSelectorSettings.instance.PreviousScenePath);
            // }) {
            //     text = "Open"
            // };
            // buttonGroup.Add(openButton);
            //
            // var playButton = new Button(() => {
            //     SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
            //     EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            //     EditorApplication.EnterPlaymode();
            // }) {
            //     text = "Play"
            // };
            // buttonGroup.Add(playButton);
            //
            // return buttonGroup;
        }
    }

    [FilePath("Assets/TN_SceneManagement/Editor/WorldGraph/SceneSelectorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SceneSelectorSettings : ScriptableSingleton<SceneSelectorSettings> {
        public string PreviousScenePath;
        private void OnDestroy() => Save(true);
    }
}