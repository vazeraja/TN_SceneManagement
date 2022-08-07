using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    [ExecuteAlways]
    public class DeleteCallback : UnityEditor.AssetModificationProcessor
    {
        static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            var objects = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in objects)
            {
                if (obj is WorldGraph b)
                {
                    foreach (var graphWindow in Resources.FindObjectsOfTypeAll< WGEditorWindow >())
                        graphWindow.OnGraphDeleted();
					
                    b.OnAssetDeleted();
                }
            }

            return AssetDeleteResult.DidNotDelete;
        }
    }
}