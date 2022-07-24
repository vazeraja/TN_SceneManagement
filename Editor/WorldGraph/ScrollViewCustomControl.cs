using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class ScrollViewCustomControl : ScrollView {
        public new class UxmlFactory : UxmlFactory<ScrollViewCustomControl, ScrollView.UxmlTraits> { }

        public void DisplayScenes() {
            GUILayout.Label("Scenes In Build", EditorStyles.boldLabel);
            for (var i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                var scene = EditorBuildSettings.scenes[i];
                if (scene.enabled)
                {
                    var sceneName = Path.GetFileNameWithoutExtension(scene.path);
                    var pressed = GUILayout.Button(i + ": " + sceneName, new GUIStyle(GUI.skin.GetStyle("Button")) { alignment = TextAnchor.MiddleLeft });
                    if (pressed)
                    {
                        if (EditorApplication.SaveCurrentSceneIfUserWantsTo())
                        {
                            EditorApplication.OpenScene(scene.path);
                        }
                    }
                }
            }
        }
    }
}