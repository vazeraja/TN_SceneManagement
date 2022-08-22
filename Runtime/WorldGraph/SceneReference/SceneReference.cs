using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    ///<summary>SceneReference is used to reference a Scene by its guid at runtime.</summary>
    [Serializable]
    public class SceneReference {
        ///<summary>The GUID that uniquely identifies this scene asset, used to serialize scene references reliably.</summary>
        ///<remarks>Even if you move/rename the scene asset, GUID references stay valid.</remarks>

        public String guid => sceneGuid;

        [SerializeField] String sceneGuid;

        ///<summary>Create a reference to a scene using its GUID.</summary>
        ///<param name="guid">The GUID of the scene, found in its .scene.meta file, or obtained from AssetDatabase.</param>
        public SceneReference(String guid) {
            this.sceneGuid = guid;
        }

        public SceneReference() { }

        ///<summary>The build index of this scene, which can be used to load it or to obtain scene info.</summary>
        #if UNITY_EDITOR
        public int sceneIndex {
            get {
                var s = UnityEditor.EditorBuildSettings.scenes;
                for (int i = 0, n = s.Length; i < n; i++)
                    if (s[i].guid.ToString() == sceneGuid) {
                        return i;
                    }

                return -1;
            }
        }
        #else
        public int sceneIndex => SceneReferences.instance.SceneIndex(sceneGuid);
        #endif
    }

}