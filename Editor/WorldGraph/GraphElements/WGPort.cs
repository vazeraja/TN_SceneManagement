using System;
using System.Reflection;
using UnityEditor.Experimental.GraphView;

namespace ThunderNut.SceneManagement.Editor {
    public class WGPort : Port {
        public WGNodeView owner { get; private set; }
        protected FieldInfo fieldInfo;
        protected BaseEdgeConnectorListener listener;

        protected WGPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(
            portOrientation, portDirection, portCapacity, type) { }
    }
}