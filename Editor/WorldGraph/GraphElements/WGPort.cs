using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace ThunderNut.SceneManagement.Editor {
    public class WGPort : Port {
        public WGGraphNode owner { get; private set; }
        public PortData portData;

        public string fieldName => fieldInfo.Name;
        public Type fieldType => fieldInfo.FieldType;
        public new Type portType;
        public event Action<WGPort, Edge> OnConnected;
        public event Action<WGPort, Edge> OnDisconnected;

        protected FieldInfo fieldInfo;
        protected BaseEdgeConnectorListener listener;

        protected WGPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(
            portOrientation, portDirection, portCapacity, type) { }
    }
}