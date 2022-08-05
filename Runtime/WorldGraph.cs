using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph")]
    public class WorldGraph : ScriptableObject, ISerializationCallbackReceiver {
        [SerializeField, Obsolete("TODO")]
        public List<JsonElement> serializedNodes = new List<JsonElement>();

        /// <summary>
        /// List of all the nodes in the graph.
        /// </summary>
        [SerializeReference]
        public List<SceneHandle> nodes = new List<SceneHandle>();

        /// <summary>
        /// Dictionary to access node per GUID, faster than a search in a list
        /// </summary>
        [NonSerialized]
        public Dictionary<string, SceneHandle> nodesPerGUID = new Dictionary<string, SceneHandle>();

        /// <summary>
        /// Json list of edges
        /// </summary>
        [SerializeField]
        public List<SerializableEdge> edges = new List<SerializableEdge>();

        /// <summary>
        /// Dictionary of edges per GUID, faster than a search in a list
        /// </summary>
        [NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new Dictionary<string, SerializableEdge>();

        public void OnBeforeSerialize() {
            throw new NotImplementedException();
        }

        public void OnAfterDeserialize() {
            throw new NotImplementedException();
        }
    }
}