using UnityEngine;

namespace ThunderNut.SceneManagement {
    [System.Serializable]
    public class SerializableEdge : ISerializationCallbackReceiver {
        public string GUID;

        [SerializeField] WorldGraph owner;

        [SerializeField] string inputNodeGUID;
        [SerializeField] string outputNodeGUID;

        [System.NonSerialized] public SceneHandle inputNode;

        [System.NonSerialized] public NodePort inputPort;
        [System.NonSerialized] public NodePort outputPort;

        [System.NonSerialized] public SceneHandle outputNode;

        public string inputFieldName;
        public string outputFieldName;

        // Use to store the id of the field that generate multiple ports
        public string inputPortIdentifier;
        public string outputPortIdentifier;

        public SerializableEdge() { }

        public static SerializableEdge CreateNewEdge(WorldGraph graph, NodePort inputPort, NodePort outputPort) {
            SerializableEdge edge = new SerializableEdge {
                owner = graph,
                GUID = System.Guid.NewGuid().ToString(),
                inputNode = inputPort.owner,
                inputFieldName = inputPort.fieldName,
                outputNode = outputPort.owner,
                outputFieldName = outputPort.fieldName,
                inputPort = inputPort,
                outputPort = outputPort,
                inputPortIdentifier = inputPort.portData.identifier,
                outputPortIdentifier = outputPort.portData.identifier
            };
            return edge;
        }

        public void OnBeforeSerialize() {
            if (outputNode == null || inputNode == null)
                return;

            outputNodeGUID = outputNode.GUID;
            inputNodeGUID = inputNode.GUID;
        }

        public void OnAfterDeserialize() { }

        //here our owner have been deserialized
        public void Deserialize() {
            if (!owner.nodesPerGUID.ContainsKey(outputNodeGUID) || !owner.nodesPerGUID.ContainsKey(inputNodeGUID))
                return;

            outputNode = owner.nodesPerGUID[outputNodeGUID];
            inputNode = owner.nodesPerGUID[inputNodeGUID];
            inputPort = inputNode.GetPort(inputFieldName, inputPortIdentifier);
            outputPort = outputNode.GetPort(outputFieldName, outputPortIdentifier);
        }

        public override string ToString() =>
            $"{outputNode.name}:{outputPort.fieldName} -> {inputNode.name}:{inputPort.fieldName}";
    }
}