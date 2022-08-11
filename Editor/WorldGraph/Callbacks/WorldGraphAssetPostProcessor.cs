using UnityEngine;
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.ShaderGraph.Drawing;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraphAssetPostProcessor : AssetPostprocessor {
        private static void NotifyEditorWindowAssetMoved(string[] movedAssets) {
            var graphWindows = Resources.FindObjectsOfTypeAll<WGEditorWindow>();

            foreach (var window in graphWindows) {
                foreach (var movedAsset in movedAssets) {
                    if (window.selectedGuid == AssetDatabase.AssetPathToGUID(movedAsset)) {
                        window.UpdateTitle();
                    }
                }
            }
        }

        private static void NotifyEditorWindowAssetDeleted(string[] deletedAssets) {
            var graphWindows = Resources.FindObjectsOfTypeAll<WGEditorWindow>();

            foreach (var window in graphWindows) {
                foreach (var deletedAsset in deletedAssets) {
                    if (window.selectedGuid == AssetDatabase.AssetPathToGUID(deletedAsset)) {
                        window.AssetWasDeleted();
                    }
                }
            }
        }

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromAssetPaths) {
            var windows = Resources.FindObjectsOfTypeAll<WGEditorWindow>();

            foreach (string importedAssetPath in importedAssets) {
                var importedObjectsAtPath = AssetDatabase.LoadAllAssetsAtPath(importedAssetPath);
                var assetGuid = AssetDatabase.AssetPathToGUID(importedAssetPath);

                foreach (var window in windows) {
                    if (window.selectedGuid == assetGuid) {
                        window.CheckForChanges();
                    }
                }

                foreach (var obj in importedObjectsAtPath) {
                    if (obj is WorldGraph graph) {
                        graph.OnAssetCreated();
                    }
                }
            }

            foreach (string movedAssetPath in movedAssets) {
                var movedObjectsAtPath = AssetDatabase.LoadAllAssetsAtPath(movedAssetPath);

                foreach (var obj in movedObjectsAtPath) {
                    if (obj is WorldGraph graph) {
                        NotifyEditorWindowAssetMoved(movedAssets);
                    }
                }
            }

            foreach (string deletedAssetPath in deletedAssets) {
                var deletedObjectsAtPath = AssetDatabase.LoadAllAssetsAtPath(deletedAssetPath);

                foreach (var obj in deletedObjectsAtPath) {
                    if (obj is WorldGraph graph) {
                        graph.OnAssetDeleted();
                        NotifyEditorWindowAssetDeleted(deletedAssets);
                    }
                }
            }
        }
    }
}