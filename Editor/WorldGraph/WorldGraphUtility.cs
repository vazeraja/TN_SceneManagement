using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace ThunderNut.SceneManagement.Editor {
    public static class WorldGraphUtility {
        public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null).ToList();
        }
    }
}