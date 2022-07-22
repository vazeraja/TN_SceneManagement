// Public Domain. NO WARRANTIES. License: https://opensource.org/licenses/0BSD

using System;
using System.Collections.Generic;
using UnityEngine;
using Guid = System.String; // could use 128-bit int type to serialize, but it would be less legible in text assets

namespace ThunderNut.SceneManagement {
    
    ///<summary>SceneReferences contains the GUID to build index mapping, used to reference scenes at runtime.</summary>
    ///<remarks>This is auto-generated on build as a preloaded asset.</remarks>
    public class SceneReferences : ScriptableObject {
        ///<summary>The singleton instance, automatically preloaded at runtime.</summary>
        public static SceneReferences instance => asset
        #if UNITY_EDITOR
        ?? (asset = ScriptableObject.CreateInstance<SceneReferences>())
        #endif
        ;

        static SceneReferences asset;

        ///<summary>The GUID for each scene asset, indexed by its build index in the array.</summary>
        public Guid[] guids => sceneGuids;

        [SerializeField] Guid[] sceneGuids;
        Dictionary<Guid, int> cache;

        private void Awake() {
            #if UNITY_EDITOR
            base.hideFlags = HideFlags.NotEditable;
            sceneGuids = Array.ConvertAll(UnityEditor.EditorBuildSettings.scenes, s => s.guid.ToString());
            #endif
            if (sceneGuids == null) sceneGuids = new Guid[0];
            int n = sceneGuids.Length;
            cache = new Dictionary<Guid, int>(n);
            for (int i = 0; i < n; i++)
                cache.Add(sceneGuids[i], i);
            asset = this;
        }

        ///<summary>The build index of a scene by GUID. It can be used to load it or to obtain scene info.</summary>
        ///<param name="sceneGuid">The GUID of the scene to look up.</param>
        ///<remarks>All mappings are cached on a dictionary when the asset is preloaded at startup.</remarks>
        public int SceneIndex(Guid sceneGuid) => cache.TryGetValue(sceneGuid, out int i) ? i : -1;
    }

    ///<summary>SceneReference is used to reference a Scene by its guid at runtime.</summary>
    [Serializable]
    public class SceneReference {
        ///<summary>The GUID that uniquely identifies this scene asset, used to serialize scene references reliably.</summary>
        ///<remarks>Even if you move/rename the scene asset, GUID references stay valid.</remarks>

        public Guid guid => sceneGuid;

        [SerializeField] Guid sceneGuid;

        ///<summary>Create a reference to a scene using its GUID.</summary>
        ///<param name="guid">The GUID of the scene, found in its .scene.meta file, or obtained from AssetDatabase.</param>
        public SceneReference(Guid guid) {
            this.sceneGuid = guid;
        }

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