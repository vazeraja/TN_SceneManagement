using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor
{
    public static class WGDependencyInstaller
    {
        static AddRequest Request;

        [MenuItem("WorldGraph/Install Dependencies")]
        static void Add()
        {
            // Add a package to the project
            Request = Client.Add("com.unity.shadergraph");
            EditorApplication.update += Progress;
        }

        static void Progress()
        {
            if (Request.IsCompleted)
            {
                if (Request.Status == StatusCode.Success)
                    Debug.Log("Installed: " + Request.Result.packageId);
                else if (Request.Status >= StatusCode.Failure)
                    Debug.Log(Request.Error.message);

                EditorApplication.update -= Progress;
            }
        } 
    }
}
