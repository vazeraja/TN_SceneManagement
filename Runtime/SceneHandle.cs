#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    [Serializable]
    public class SceneConnection {
        public int passage;
        public SceneHandle sceneHandle;
        public int sceneHandlePassage;
    }

    [CreateAssetMenu(fileName = "SceneHandle", menuName = "World Graph/Default Scene Handle")]
    public class DefaultSceneHandle : SceneHandle {
        protected override void ForceSwitchToScene() {
            base.ForceSwitchToScene();
            Debug.Log("");
        }
    }

    public abstract class SceneHandle : ScriptableObject {
        [SerializeField, HideInInspector] internal string nodeCustomName = null;
        [NonSerialized] bool _needsInspector = false;
        [HideInInspector] public string GUID;
        [HideInInspector] public Rect position;
        [HideInInspector] public bool expanded;
        [HideInInspector] public bool debug;
        [HideInInspector] public bool nodeLock;
        public virtual string m_Name => GetType().Name;
        public virtual Color color => Color.clear;
        public virtual string layoutStyle => string.Empty;
        public virtual bool unlockable => true;
        public virtual bool isLocked => nodeLock;
        public virtual bool showControlsOnHover => false;
        public virtual bool deletable => true;
        public bool createdFromDuplication { get; internal set; } = false;
        public bool createdWithinGroup { get; internal set; } = false;
        public virtual bool needsInspector => _needsInspector;
        public virtual bool isRenamable => false;

        [NonSerialized]
        internal Dictionary<string, NodeFieldInformation> nodeFields = new Dictionary<string, NodeFieldInformation>();

        internal class NodeFieldInformation {
            public string name;
            public string fieldName;
            public FieldInfo info;
            public bool input;
            public bool isMultiple;
            public string tooltip;
            public bool vertical;

            public NodeFieldInformation(FieldInfo info, string name, bool input, bool isMultiple, string tooltip,
                bool vertical) {
                this.input = input;
                this.isMultiple = isMultiple;
                this.info = info;
                this.name = name;
                this.fieldName = info.Name;
                this.tooltip = tooltip;
                this.vertical = vertical;
            }
        }

        [NonSerialized] public readonly NodeInputPortContainer inputPorts;

        [NonSerialized] public readonly NodeOutputPortContainer outputPorts;

        public event Action<SerializableEdge> onAfterEdgeConnected;
        public event Action<SerializableEdge> onAfterEdgeDisconnected;

        public WorldGraph graph;

        public void OnEdgeConnected(SerializableEdge edge) {
            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer) inputPorts : outputPorts;

            portCollection.Add(edge);

            onAfterEdgeConnected?.Invoke(edge);
        }

        protected virtual bool CanResetPort(NodePort port) {
            return true;
        }

        public void OnEdgeDisconnected(SerializableEdge edge) {
            if (edge == null)
                return;

            bool input = edge.inputNode == this;
            NodePortContainer portCollection = (input) ? (NodePortContainer) inputPorts : outputPorts;

            portCollection.Remove(edge);

            // Reset default values of input port:
            bool haveConnectedEdges = edge.inputNode.inputPorts.Where(p => p.fieldName == edge.inputFieldName)
                .Any(p => p.GetEdges().Count != 0);
            if (edge.inputNode == this && !haveConnectedEdges && CanResetPort(edge.inputPort))
                edge.inputPort?.ResetToDefault();


            onAfterEdgeDisconnected?.Invoke(edge);
        }

        /// <summary>
        /// Add a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="fieldName">C# field name</param>
        /// <param name="portData">Data of the port</param>
        public void AddPort(bool input, string fieldName, PortData portData) {
            // Fixup port data info if needed:
            if (portData.displayType == null)
                portData.displayType = nodeFields[fieldName].info.FieldType;

            if (input)
                inputPorts.Add(new NodePort(this, fieldName, portData));
            else
                outputPorts.Add(new NodePort(this, fieldName, portData));
        }

        /// <summary>
        /// Remove a port
        /// </summary>
        /// <param name="input">is input port</param>
        /// <param name="port">the port to delete</param>
        public void RemovePort(bool input, NodePort port) {
            if (input)
                inputPorts.Remove(port);
            else
                outputPorts.Remove(port);
        }

        /// <summary>
        /// Remove port(s) from field name
        /// </summary>
        /// <param name="input">is input</param>
        /// <param name="fieldName">C# field name</param>
        public void RemovePort(bool input, string fieldName) {
            if (input)
                inputPorts.RemoveAll(p => p.fieldName == fieldName);
            else
                outputPorts.RemoveAll(p => p.fieldName == fieldName);
        }

        /// <summary>
        /// Get all the nodes connected to the input ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<SceneHandle> GetInputNodes() {
            return from port in inputPorts from edge in port.GetEdges() select edge.outputNode;
        }

        /// <summary>
        /// Get all the nodes connected to the output ports of this node
        /// </summary>
        /// <returns>an enumerable of node</returns>
        public IEnumerable<SceneHandle> GetOutputNodes() {
            return from port in outputPorts from edge in port.GetEdges() select edge.inputNode;
        }

        /// <summary>
        /// Get the port from field name and identifier
        /// </summary>
        public NodePort GetPort(string fieldName, string identifier) {
            return inputPorts.Concat(outputPorts).FirstOrDefault(p => {
                var bothNull = String.IsNullOrEmpty(identifier) && String.IsNullOrEmpty(p.portData.identifier);
                return p.fieldName == fieldName && (bothNull || identifier == p.portData.identifier);
            });
        }

        public void SetCustomName(string customName) => nodeCustomName = customName;
        public string GetCustomName() => string.IsNullOrEmpty(nodeCustomName) ? name : nodeCustomName;

        #region Runtime Code

        public SceneReference scene;
        public List<string> passages = new List<string> {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;

        protected virtual void ForceSwitchToScene() {
            SceneManager.LoadScene(scene.sceneIndex);
        }

        #endregion
    }
}