using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace ThunderNut.SceneManagement {
    public class GraphChanges {
        public SerializableEdge removedEdge;
        public SerializableEdge addedEdge;
        public SceneHandle removedNode;
        public SceneHandle addedNode;
        public SceneHandle nodeChanged;
        public Group addedGroups;
        public Group removedGroups;
        public BaseStackNode addedStackNode;
        public BaseStackNode removedStackNode;
        public StickyNote addedStickyNotes;
        public StickyNote removedStickyNotes;
    }

    [CreateAssetMenu(fileName = "WorldGraph", menuName = "World Graph/World Graph"), Serializable]
    public class WorldGraph : ScriptableObject, ISerializationCallbackReceiver {
        [SerializeField, Obsolete("TODO")]
        public List<JsonElement> serializedNodes = new List<JsonElement>();

        [SerializeReference]
        public List<SceneHandle> nodes = new List<SceneHandle>();

        [NonSerialized]
        public Dictionary<string, SceneHandle> nodesPerGUID = new Dictionary<string, SceneHandle>();

        [SerializeField]
        public List<SerializableEdge> edges = new List<SerializableEdge>();

        [NonSerialized]
        public Dictionary<string, SerializableEdge> edgesPerGUID = new Dictionary<string, SerializableEdge>();

        [SerializeField]
        public List<Group> groups = new List<Group>();

        [SerializeField, SerializeReference] // Polymorphic serialization
        public List<BaseStackNode> stackNodes = new List<BaseStackNode>();

        [SerializeField]
        public List<PinnedElement> pinnedElements = new List<PinnedElement>();

        [SerializeField, SerializeReference]
        public List<ExposedParameter> exposedParameters = new List<ExposedParameter>();

        [SerializeField]
        public List<StickyNote> stickyNotes = new List<StickyNote>();

        public Vector3 position = Vector3.zero;
        public Vector3 scale = Vector3.one;

        public event Action onExposedParameterListChanged;
        public event Action<ExposedParameter> onExposedParameterModified;
        public event Action<ExposedParameter> onExposedParameterValueChanged;

        public event Action onEnabled;

        public event Action<GraphChanges> onGraphChanges;

        [field: NonSerialized]
        public bool isEnabled { get; private set; } = false;

        protected virtual void OnEnable() {
            if (isEnabled)
                OnDisable();

            InitializeGraphElements();
            DestroyBrokenGraphElements();
            
            isEnabled = true;
            onEnabled?.Invoke();
        }

        private void InitializeGraphElements() {
            // Sanitize the element lists (it's possible that nodes are null if their full class name have changed)
            // If you rename / change the assembly of a node or parameter, please use the MovedFrom() attribute to avoid breaking the graph.
            nodes.RemoveAll(n => n == null);
            exposedParameters.RemoveAll(e => e == null);

            foreach (var node in nodes.ToList()) {
                nodesPerGUID[node.GUID] = node;
                node.Initialize(this);
            }

            foreach (var edge in edges.ToList()) {
                edge.Deserialize();
                edgesPerGUID[edge.GUID] = edge;

                // Sanity check for the edge:
                if (edge.inputPort == null || edge.outputPort == null) {
                    Disconnect(edge.GUID);
                    continue;
                }

                // Add the edge to the non-serialized port data
                edge.inputPort.owner.OnEdgeConnected(edge);
                edge.outputPort.owner.OnEdgeConnected(edge);
            }
        }

        private void DestroyBrokenGraphElements() {
            edges.RemoveAll(e => e.inputNode == null
                                 || e.outputNode == null
                                 || string.IsNullOrEmpty(e.outputFieldName)
                                 || string.IsNullOrEmpty(e.inputFieldName)
            );
            nodes.RemoveAll(n => n == null);
        }

        protected virtual void OnDisable() {
            isEnabled = false;
            foreach (var node in nodes)
                node.DisableInternal();
        }
        public virtual void OnAssetCreated() {
            Debug.Log("WorldGraph SO Created");
        }

        public virtual void OnAssetDeleted() {
            Debug.Log("WorldGraph SO Deleted");
        }

        public SceneHandle AddNode(SceneHandle node) {
            nodesPerGUID[node.GUID] = node;

            nodes.Add(node);
            node.Initialize(this);
            ExceptionToLog.Call(node.OnNodeCreated);
            
            onGraphChanges?.Invoke(new GraphChanges {addedNode = node});

            return node;
        }

        public void RemoveNode(SceneHandle node) {
            node.DisableInternal();
            node.DestroyInternal();

            nodesPerGUID.Remove(node.GUID);
            nodes.Remove(node);

            onGraphChanges?.Invoke(new GraphChanges {removedNode = node});
        }

        public SerializableEdge Connect(NodePort inputPort, NodePort outputPort, bool autoDisconnectInputs = true) {
            var edge = SerializableEdge.CreateNewEdge(this, inputPort, outputPort);

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !inputPort.portData.acceptMultipleEdges) {
                foreach (var e in inputPort.GetEdges().ToList()) {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }

            // same for the output port:
            if (autoDisconnectInputs && !outputPort.portData.acceptMultipleEdges) {
                foreach (var e in outputPort.GetEdges().ToList()) {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    Disconnect(e);
                }
            }

            edges.Add(edge);

            // Add the edge to the list of connected edges in the nodes
            inputPort.owner.OnEdgeConnected(edge);
            outputPort.owner.OnEdgeConnected(edge);

            onGraphChanges?.Invoke(new GraphChanges {addedEdge = edge});

            return edge;
        }

        public void Disconnect(SceneHandle inputNode, string inputFieldName, SceneHandle outputNode,
            string outputFieldName) {
            edges.RemoveAll(r => {
                bool remove = r.inputNode == inputNode
                              && r.outputNode == outputNode
                              && r.outputFieldName == outputFieldName
                              && r.inputFieldName == inputFieldName;

                switch (remove) {
                    case true:
                        r.inputNode?.OnEdgeDisconnected(r);
                        r.outputNode?.OnEdgeDisconnected(r);
                        onGraphChanges?.Invoke(new GraphChanges {removedEdge = r});
                        break;
                }

                return remove;
            });
        }

        public void Disconnect(SerializableEdge edge) => Disconnect(edge.GUID);

        public void Disconnect(string edgeGUID) {
            List<(SceneHandle, SerializableEdge)> disconnectEvents = new List<(SceneHandle, SerializableEdge)>();

            edges.RemoveAll(r => {
                if (r.GUID == edgeGUID) {
                    disconnectEvents.Add((r.inputNode, r));
                    disconnectEvents.Add((r.outputNode, r));
                    onGraphChanges?.Invoke(new GraphChanges {removedEdge = r});
                }

                return r.GUID == edgeGUID;
            });

            // Delay the edge disconnect event to avoid recursion
            foreach (var (node, edge) in disconnectEvents)
                node?.OnEdgeDisconnected(edge);
        }

        public void AddGroup(Group block) {
            groups.Add(block);
            onGraphChanges?.Invoke(new GraphChanges {addedGroups = block});
        }

        public void RemoveGroup(Group block) {
            groups.Remove(block);
            onGraphChanges?.Invoke(new GraphChanges {removedGroups = block});
        }

        public void AddStackNode(BaseStackNode stackNode) {
            stackNodes.Add(stackNode);
            onGraphChanges?.Invoke(new GraphChanges {addedStackNode = stackNode});
        }

        public void RemoveStackNode(BaseStackNode stackNode) {
            stackNodes.Remove(stackNode);
            onGraphChanges?.Invoke(new GraphChanges {removedStackNode = stackNode});
        }

        public void AddStickyNote(StickyNote note) {
            stickyNotes.Add(note);
            onGraphChanges?.Invoke(new GraphChanges {addedStickyNotes = note});
        }

        public void RemoveStickyNote(StickyNote note) {
            stickyNotes.Remove(note);
            onGraphChanges?.Invoke(new GraphChanges {removedStickyNotes = note});
        }

        public PinnedElement OpenPinned(Type viewType) {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            if (pinned == null) {
                pinned = new PinnedElement(viewType);
                pinnedElements.Add(pinned);
            }
            else
                pinned.opened = true;

            return pinned;
        }

        public void ClosePinned(Type viewType) {
            var pinned = pinnedElements.Find(p => p.editorType.type == viewType);

            pinned.opened = false;
        }

        public string AddExposedParameter(string name, Type type, object value = null) {
            if (!type.IsSubclassOf(typeof(ExposedParameter))) {
                Debug.LogError($"Can't add parameter of type {type}, the type doesn't inherit from ExposedParameter.");
            }

            var param = Activator.CreateInstance(type) as ExposedParameter;

            // patch value with correct type:
            if (param.GetValueType().IsValueType)
                value = Activator.CreateInstance(param.GetValueType());

            param.Initialize(name, value);
            exposedParameters.Add(param);

            onExposedParameterListChanged?.Invoke();

            return param.guid;
        }

        public string AddExposedParameter(ExposedParameter parameter) {
            string guid = Guid.NewGuid().ToString(); // Generated once and unique per parameter

            parameter.guid = guid;
            exposedParameters.Add(parameter);

            onExposedParameterListChanged?.Invoke();

            return guid;
        }

        public void RemoveExposedParameter(ExposedParameter ep) {
            exposedParameters.Remove(ep);

            onExposedParameterListChanged?.Invoke();
        }

        public void RemoveExposedParameter(string guid) {
            if (exposedParameters.RemoveAll(e => e.guid == guid) != 0)
                onExposedParameterListChanged?.Invoke();
        }

        internal void NotifyExposedParameterListChanged()
            => onExposedParameterListChanged?.Invoke();

        /// <summary>
        /// Update an exposed parameter value
        /// </summary>
        /// <param name="guid">GUID of the parameter</param>
        /// <param name="value">new value</param>
        public void UpdateExposedParameter(string guid, object value) {
            var param = exposedParameters.Find(e => e.guid == guid);
            if (param == null)
                return;

            if (value != null && !param.GetValueType().IsAssignableFrom(value.GetType()))
                throw new Exception("Type mismatch when updating parameter " + param.name + ": from " +
                                    param.GetValueType() + " to " + value.GetType().AssemblyQualifiedName);

            param.value = value;
            onExposedParameterModified?.Invoke(param);
        }

        public void UpdateExposedParameterName(ExposedParameter parameter, string name) {
            parameter.name = name;
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterChanged(ExposedParameter parameter) {
            onExposedParameterModified?.Invoke(parameter);
        }

        public void NotifyExposedParameterValueChanged(ExposedParameter parameter) {
            onExposedParameterValueChanged?.Invoke(parameter);
        }

        public ExposedParameter GetExposedParameter(string name) {
            return exposedParameters.FirstOrDefault(e => e.name == name);
        }

        public ExposedParameter GetExposedParameterFromGUID(string guid) {
            return exposedParameters.FirstOrDefault(e => e?.guid == guid);
        }

        public bool SetParameterValue(string name, object value) {
            var e = exposedParameters.FirstOrDefault(p => p.name == name);

            if (e == null)
                return false;

            e.value = value;

            return true;
        }

        public object GetParameterValue(string name) => exposedParameters.FirstOrDefault(p => p.name == name)?.value;

        public T GetParameterValue<T>(string name) => (T) GetParameterValue(name);

        public void OnBeforeSerialize() {
            // Cleanup broken elements
            stackNodes.RemoveAll(s => s == null);
            nodes.RemoveAll(n => n == null);
        }

        public void Deserialize() {
            // Disable nodes correctly before removing them:
            if (nodes != null) {
                foreach (var node in nodes)
                    node.DisableInternal();
            }

            InitializeGraphElements();
        }

        public void OnAfterDeserialize() { }
    }
}