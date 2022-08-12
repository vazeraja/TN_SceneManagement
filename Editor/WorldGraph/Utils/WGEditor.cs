using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace ThunderNut.SceneManagement.Editor {
    public static class WGEditor {
        public static void HorizontalScope(Action block) {
            EditorGUILayout.BeginHorizontal();
            block();
            EditorGUILayout.EndHorizontal();
        }

        public static void VerticalScope(Action block) {
            EditorGUILayout.BeginVertical();
            block();
            EditorGUILayout.EndVertical();
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null).ToList();
        }
    }
}