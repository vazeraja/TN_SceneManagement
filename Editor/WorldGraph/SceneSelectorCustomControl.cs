using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
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

        private VisualElement CreateSceneButton(string sceneGuid) {
            string scenePath = AssetDatabase.GUIDToAssetPath(sceneGuid);
            var sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            var buttonGroup = new IMGUIContainer();
            buttonGroup.style.flexDirection = FlexDirection.Row;
            buttonGroup.style.marginLeft = 3;

            var label = new Label($"{sceneAsset.name}");
            label.style.width = 150;
            buttonGroup.Add(label);

            var openButton = new Button(() => {
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
                Debug.Log(SceneSelectorSettings.instance.PreviousScenePath);
            }) {
                text = "Open"
            };
            buttonGroup.Add(openButton);

            var playButton = new Button(() => {
                SceneSelectorSettings.instance.PreviousScenePath = SceneManager.GetActiveScene().path;
                EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                EditorApplication.EnterPlaymode();
            }) {
                text = "Play"
            };
            buttonGroup.Add(playButton);

            return buttonGroup;
        }
    }

    [FilePath("Assets/TN_SceneManagement/Editor/WorldGraph/SceneSelectorSettings.asset", FilePathAttribute.Location.ProjectFolder)]
    public class SceneSelectorSettings : ScriptableSingleton<SceneSelectorSettings> {
        public string PreviousScenePath;
        private void OnDestroy() => Save(true);
    }
}