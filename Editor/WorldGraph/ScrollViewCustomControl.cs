using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {
    public class ScrollViewCustomControl : ScrollView {
        public new class UxmlFactory : UxmlFactory<ScrollViewCustomControl, ScrollView.UxmlTraits> { }

        // Rect buttonRect;
        // public void CreateSceneGUI() {
        //     Add(new IMGUIContainer(() => {
        //         GUILayout.Label("Editor window with popup", EditorStyles.boldLabel);
        //         if (GUILayout.Button("Popup Window")) {
        //             PopupWindow.Show(buttonRect, new TreeViewPopupWindow(new SimpleTreeView(), buttonRect.width));
        //         }
        //
        //         if (Event.current.type == EventType.Repaint)
        //             buttonRect = GUILayoutUtility.GetLastRect();
        //     }));
        //     
        //     string[] sceneGuids = Array.ConvertAll(UnityEditor.EditorBuildSettings.scenes, s => s.guid.ToString());
        //     foreach (string sceneGuid in sceneGuids) {
        //         Add(CreateSceneButton(sceneGuid));
        //     }
        // }
        
        // [InitializeOnLoadMethod]
        // private static void RegisterCallbacks() {
        //     EditorApplication.playModeStateChanged += ReturnToPreviousScene;
        // }
        //
        // private static void ReturnToPreviousScene(PlayModeStateChange change) {
        //     switch (change) {
        //         case PlayModeStateChange.EnteredEditMode:
        //             // EditorSceneManager.OpenScene(SceneSelectorSettings.instance.PreviousScenePath, OpenSceneMode.Single);
        //             break;
        //         case PlayModeStateChange.EnteredPlayMode:
        //             break;
        //         case PlayModeStateChange.ExitingEditMode:
        //             break;
        //         case PlayModeStateChange.ExitingPlayMode:
        //             break;
        //         default:
        //             throw new ArgumentOutOfRangeException(nameof(change), change, null);
        //     }
        // }

        // private IMGUIContainer CreateSceneButton(string sceneGuid = "") {
        //     var buttonGroup = new IMGUIContainer();
        //
        //     Scene Selector Code
        //     string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
        //     var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
        //     
        //     buttonGroup.style.flexDirection = FlexDirection.Row;
        //     buttonGroup.style.marginLeft = 3;
        //     
        //     var label = new Label($"{sceneAsset.name}");
        //     label.style.width = 150;
        //     buttonGroup.Add(label);
        //
        //     var openButton = new Button(() => {
        //         EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        //         SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
        //         Debug.Log(SceneSelectorSettings.instance.PreviousScenePath);
        //     }) {
        //         text = "Open"
        //     };
        //     buttonGroup.Add(openButton);
        //     
        //     var playButton = new Button(() => {
        //         SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
        //         EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
        //         EditorApplication.EnterPlaymode();
        //     }) {
        //         text = "Play"
        //     };
        //     buttonGroup.Add(playButton);
        //     
        //     return buttonGroup;
        // }
    }

    [FilePath("Assets/TN_SceneManagement/Editor/WorldGraph/SceneSelectorSettings.asset",
        FilePathAttribute.Location.ProjectFolder)]
    public class SceneSelectorSettings : ScriptableSingleton<SceneSelectorSettings> {
        public string PreviousScenePath;
        private void OnDestroy() => Save(true);
    }
}