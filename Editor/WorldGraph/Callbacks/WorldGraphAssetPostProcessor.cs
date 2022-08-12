using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;

namespace ThunderNut.SceneManagement.Editor {
    [ExecuteAlways]
    public class DeleteCallback : AssetModificationProcessor {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options) {
            var objects = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objects) {
                if (obj is WorldGraph b) {
                    b.OnAssetDeleted();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }

    public class WorldGraphAssetPostProcessor : AssetPostprocessor {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths) { }
    }
}