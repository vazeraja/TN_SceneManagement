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
        public static SceneReferences 
            instance => asset
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

}