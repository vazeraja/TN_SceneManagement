using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGraphView : GraphView, IDisposable {
        private const string styleSheetPath = "Styles/WorldGraphEditorWindow";

        public delegate void NodeDuplicatedDelegate(SceneHandle duplicatedNode, SceneHandle newNode);

        public WorldGraph graph;
        public EditorWindow m_EditorWindow;

        public List<WGGraphNode> nodeViews = new List<WGGraphNode>();
        public Dictionary<SceneHandle, WGGraphNode> nodeViewsPerNode = new Dictionary<SceneHandle, WGGraphNode>();
        public List<WGEdge> edgeViews = new List<WGEdge>();
        public List<WGGroup> groupViews = new List<WGGroup>();
        public List<WGStackNode> stackNodeViews = new List<WGStackNode>();
        public Dictionary<Type, WGPinnedElement> pinnedElements = new Dictionary<Type, WGPinnedElement>();
        #if UNITY_2020_1_OR_NEWER
        public List<WGStickyNote> stickyNoteViews = new List<WGStickyNote>();
        #endif

        public BaseEdgeConnectorListener connectorListener;
        private SearchWindowProvider m_SearchWindowProvider;

        public event Action initialized;
        public event Action onExposedParameterListChanged;

        public event NodeDuplicatedDelegate nodeDuplicated;

        public SerializedObject serializedGraph { get; private set; }

        public WGGraphView(EditorWindow window) {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphViewX"));

            m_EditorWindow = window;

            serializeGraphElements = SerializeGraphElementsCallback;
            canPasteSerializedData = CanPasteSerializedDataCallback;
            unserializeAndPaste = UnserializeAndPasteCallback;
            //graphViewChanged = GraphViewChangedCallback;
            viewTransformChanged = ViewTransformChangedCallback;
            elementResized = ElementResizedCallback;

            // Add RegisterCallbacks for Mouse and Keyboard events

            m_SearchWindowProvider = ScriptableObject.CreateInstance<WGSearcherProvider>();
            m_SearchWindowProvider.Initialize(m_EditorWindow, this);
            nodeCreationRequest = NodeCreationRequest;
        }

        #region Callbacks

        void NodeCreationRequest(NodeCreationContext c) {
            if (EditorWindow.focusedWindow == m_EditorWindow) {
                var displayPosition = (c.screenMousePosition - m_EditorWindow.position.position);

                m_SearchWindowProvider.target = c.target;
                SearcherWindow.Show(m_EditorWindow, ((WGSearcherProvider) m_SearchWindowProvider).LoadSearchWindow(),
                    item => ((WGSearcherProvider) m_SearchWindowProvider).OnSearcherSelectEntry(item,
                        c.screenMousePosition - m_EditorWindow.position.position),
                    displayPosition, null);
            }
        }

        protected override bool canCopySelection {
            get { return selection.Any(e => e is WGGraphNode || e is WGGroup); }
        }

        protected override bool canCutSelection {
            get { return selection.Any(e => e is WGGraphNode || e is WGGroup); }
        }

        private string SerializeGraphElementsCallback(IEnumerable<GraphElement> elements) {
            var data = new CopyPasteHelper();

            var enumerable = elements as GraphElement[] ?? elements.ToArray();
            foreach (var graphElement in enumerable.Where(e => e is WGGraphNode)) {
                var nodeView = (WGGraphNode) graphElement;
                data.copiedNodes.Add(JsonSerializer.SerializeNode(nodeView.nodeTarget));
                foreach (var port in nodeView.nodeTarget.GetAllPorts()) {
                    if (port.portData.vertical) {
                        foreach (var edge in port.GetEdges())
                            data.copiedEdges.Add(JsonSerializer.Serialize(edge));
                    }
                }
            }

            foreach (var graphElement in enumerable.Where(e => e is WGGroup)) {
                var groupView = (WGGroup) graphElement;
                data.copiedGroups.Add(JsonSerializer.Serialize(groupView.@group));
            }

            foreach (var graphElement in enumerable.Where(e => e is WGEdge)) {
                var edgeView = (WGEdge) graphElement;
                data.copiedEdges.Add(JsonSerializer.Serialize(edgeView.serializedEdge));
            }

            ClearSelection();

            return JsonUtility.ToJson(data, true);
        }

        private bool CanPasteSerializedDataCallback(string serializedData) {
            try {
                return JsonUtility.FromJson(serializedData, typeof(CopyPasteHelper)) != null;
            }
            catch {
                return false;
            }
        }

        void UnserializeAndPasteCallback(string operationName, string serializedData) {
            var data = JsonUtility.FromJson<CopyPasteHelper>(serializedData);

            RegisterCompleteObjectUndo(operationName);

            Dictionary<string, SceneHandle> copiedNodesMap = new Dictionary<string, SceneHandle>();

            var unserializedGroups = data.copiedGroups.Select(g => JsonSerializer.Deserialize<Group>(g)).ToList();

            foreach (var serializedNode in data.copiedNodes) {
                var node = JsonSerializer.DeserializeNode(serializedNode);

                if (node == null)
                    continue;

                string sourceGUID = node.GUID;
                graph.nodesPerGUID.TryGetValue(sourceGUID, out var sourceNode);
                //Call OnNodeCreated on the new fresh copied node
                node.createdFromDuplication = true;
                node.createdWithinGroup = unserializedGroups.Any(g => g.innerNodeGUIDs.Contains(sourceGUID));
                node.OnNodeCreated();
                //And move a bit the new node
                node.position.position += new Vector2(20, 20);

                var newNodeView = AddNode(node);

                // If the nodes were copied from another graph, then the source is null
                if (sourceNode != null)
                    nodeDuplicated?.Invoke(sourceNode, node);
                copiedNodesMap[sourceGUID] = node;

                //Select the new node
                AddToSelection(nodeViewsPerNode[node]);
            }

            foreach (var group in unserializedGroups) {
                //Same than for node
                group.OnCreated();

                // try to centre the created node in the screen
                group.position.position += new Vector2(20, 20);

                var oldGUIDList = group.innerNodeGUIDs.ToList();
                group.innerNodeGUIDs.Clear();
                foreach (var guid in oldGUIDList) {
                    graph.nodesPerGUID.TryGetValue(guid, out var node);

                    // In case group was copied from another graph
                    if (node == null) {
                        copiedNodesMap.TryGetValue(guid, out node);
                        group.innerNodeGUIDs.Add(node.GUID);
                    }
                    else {
                        group.innerNodeGUIDs.Add(copiedNodesMap[guid].GUID);
                    }
                }

                AddGroup(group);
            }

            foreach (var serializedEdge in data.copiedEdges) {
                var edge = JsonSerializer.Deserialize<SerializableEdge>(serializedEdge);

                edge.Deserialize();

                // Find port of new nodes:
                copiedNodesMap.TryGetValue(edge.inputNode.GUID, out var oldInputNode);
                copiedNodesMap.TryGetValue(edge.outputNode.GUID, out var oldOutputNode);

                // We avoid to break the graph by replacing unique connections:
                if (oldInputNode == null && !edge.inputPort.portData.acceptMultipleEdges ||
                    !edge.outputPort.portData.acceptMultipleEdges)
                    continue;

                oldInputNode = oldInputNode ? oldInputNode : edge.inputNode;
                oldOutputNode = oldOutputNode ? oldOutputNode : edge.outputNode;

                NodePort inputPort = oldInputNode.GetPort(edge.inputPort.fieldName, edge.inputPortIdentifier);
                NodePort outputPort = oldOutputNode.GetPort(edge.outputPort.fieldName, edge.outputPortIdentifier);

                var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

                if (nodeViewsPerNode.ContainsKey(oldInputNode) && nodeViewsPerNode.ContainsKey(oldOutputNode)) {
                    var edgeView = CreateEdgeView();
                    edgeView.userData = newEdge;
                    edgeView.input = nodeViewsPerNode[oldInputNode]
                        .GetPortViewFromFieldName(newEdge.inputFieldName, newEdge.inputPortIdentifier);
                    edgeView.output = nodeViewsPerNode[oldOutputNode]
                        .GetPortViewFromFieldName(newEdge.outputFieldName, newEdge.outputPortIdentifier);

                    Connect(edgeView);
                }
            }
        }

        private void ViewTransformChangedCallback(GraphView view) {
            if (graph == null) return;
            graph.position = viewTransform.position;
            graph.scale = viewTransform.scale;
        }

        private void ElementResizedCallback(VisualElement elem) {
            if (elem is WGGroup groupView)
                groupView.group.size = groupView.GetPosition().size;
        }

        #endregion

        public void Initialize(WorldGraph graph) {
            Debug.Log("WGGraphView: Initialized");
        }

        public WGGraphNode AddNode(SceneHandle node) {
            // This will initialize the node using the graph instance
            graph.AddNode(node);

            UpdateSerializedProperties();

            var view = AddNodeView(node);

            // Call create after the node have been initialized
            ExceptionToLog.Call(() => view.OnCreated());

            return view;
        }

        public WGGraphNode AddNodeView(SceneHandle node) {
            var baseNodeView = Activator.CreateInstance(typeof(WGGraphNode)) as WGGraphNode;
            baseNodeView?.Initialize(this, node);
            AddElement(baseNodeView);

            nodeViews.Add(baseNodeView);
            nodeViewsPerNode[node] = baseNodeView;

            return baseNodeView;
        }

        public virtual WGEdge CreateEdgeView() {
            return new WGEdge();
        }

        public bool CanConnectEdge(WGEdge e, bool autoDisconnectInputs = true) {
            if (e.input == null || e.output == null)
                return false;

            var inputPortView = e.input as WGPort;
            var outputPortView = e.output as WGPort;
            var inputNodeView = inputPortView.node as WGGraphNode;
            var outputNodeView = outputPortView.node as WGGraphNode;

            if (inputNodeView == null || outputNodeView == null) {
                Debug.LogError("Connect aborted !");
                return false;
            }

            return true;
        }

        public bool ConnectView(WGEdge e, bool autoDisconnectInputs = true) {
            if (!CanConnectEdge(e, autoDisconnectInputs))
                return false;

            var inputPortView = e.input as WGPort;
            var outputPortView = e.output as WGPort;
            var inputNodeView = inputPortView.node as WGGraphNode;
            var outputNodeView = outputPortView.node as WGGraphNode;

            //If the input port does not support multi-connection, we remove them
            if (autoDisconnectInputs && !(e.input as WGPort).portData.acceptMultipleEdges) {
                foreach (var edge in edgeViews.Where(ev => ev.input == e.input).ToList()) {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }

            // same for the output port:
            if (autoDisconnectInputs && !(e.output as WGPort).portData.acceptMultipleEdges) {
                foreach (var edge in edgeViews.Where(ev => ev.output == e.output).ToList()) {
                    // TODO: do not disconnect them if the connected port is the same than the old connected
                    DisconnectView(edge);
                }
            }

            AddElement(e);

            e.input.Connect(e);
            e.output.Connect(e);

            // If the input port have been removed by the custom port behavior
            // we try to find if it's still here
            if (e.input == null)
                e.input = inputNodeView.GetPortViewFromFieldName(inputPortView.fieldName,
                    inputPortView.portData.identifier);
            if (e.output == null)
                e.output = inputNodeView.GetPortViewFromFieldName(outputPortView.fieldName,
                    outputPortView.portData.identifier);

            edgeViews.Add(e);

            inputNodeView.RefreshPorts();
            outputNodeView.RefreshPorts();

            // In certain cases the edge color is wrong so we patch it
            schedule.Execute(() => { e.UpdateEdgeControl(); }).ExecuteLater(1);

            e.isConnected = true;

            return true;
        }

        public bool Connect(WGPort inputPortView, WGPort outputPortView, bool autoDisconnectInputs = true) {
            var inputPort =
                inputPortView.owner.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
            var outputPort =
                outputPortView.owner.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

            // Checks that the node we are connecting still exists
            if (inputPortView.owner.parent == null || outputPortView.owner.parent == null)
                return false;

            var newEdge = SerializableEdge.CreateNewEdge(graph, inputPort, outputPort);

            var edgeView = CreateEdgeView();
            edgeView.userData = newEdge;
            edgeView.input = inputPortView;
            edgeView.output = outputPortView;


            return Connect(edgeView);
        }

        public bool Connect(WGEdge e, bool autoDisconnectInputs = true) {
            if (!CanConnectEdge(e, autoDisconnectInputs))
                return false;

            var inputPortView = e.input as WGPort;
            var outputPortView = e.output as WGPort;
            var inputNodeView = inputPortView.node as WGGraphNode;
            var outputNodeView = outputPortView.node as WGGraphNode;
            var inputPort =
                inputNodeView.nodeTarget.GetPort(inputPortView.fieldName, inputPortView.portData.identifier);
            var outputPort =
                outputNodeView.nodeTarget.GetPort(outputPortView.fieldName, outputPortView.portData.identifier);

            e.userData = graph.Connect(inputPort, outputPort, autoDisconnectInputs);

            ConnectView(e, autoDisconnectInputs);

            return true;
        }

        public void DisconnectView(WGEdge e, bool refreshPorts = true) {
            if (e == null)
                return;

            RemoveElement(e);

            if (e?.input?.node is WGGraphNode inputNodeView) {
                e.input.Disconnect(e);
                if (refreshPorts)
                    inputNodeView.RefreshPorts();
            }

            if (e?.output?.node is WGGraphNode outputNodeView) {
                e.output.Disconnect(e);
                if (refreshPorts)
                    outputNodeView.RefreshPorts();
            }

            edgeViews.Remove(e);
        }

        public void Disconnect(WGEdge e, bool refreshPorts = true) {
            // Remove the serialized edge if there is one
            if (e.userData is SerializableEdge serializableEdge)
                graph.Disconnect(serializableEdge.GUID);

            DisconnectView(e, refreshPorts);
        }

        public WGGroup AddGroup(Group block) {
            graph.AddGroup(block);
            block.OnCreated();
            return AddGroupView(block);
        }

        public WGGroup AddGroupView(Group block) {
            var c = new WGGroup();

            c.Initialize(this, block);

            AddElement(c);

            groupViews.Add(c);
            return c;
        }

        public void RegisterCompleteObjectUndo(string name) {
            Undo.RegisterCompleteObjectUndo(graph, name);
        }

        private void UpdateSerializedProperties() {
            serializedGraph = new SerializedObject(graph);
        }

        public void SaveGraphToDisk() {
            if (graph == null)
                return;

            EditorUtility.SetDirty(graph);
        }

        public void Dispose() {
            Debug.Log("Disposing Graph View");

            // ClearGraphElements();
            // RemoveFromHierarchy();
            // Undo.undoRedoPerformed -= ReloadView;
            // Object.DestroyImmediate(nodeInspector);
            // NodeProvider.UnloadGraph(graph);
            // exposedParameterFactory.Dispose();
            // exposedParameterFactory = null;
            // 
            // graph.onExposedParameterListChanged -= OnExposedParameterListChanged;
            // graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            // graph.onGraphChanges -= GraphChangesCallback;

            nodeCreationRequest = null;

            if (m_SearchWindowProvider != null) {
                Object.DestroyImmediate(m_SearchWindowProvider);
                m_SearchWindowProvider = null;
            }
        }
    }
}