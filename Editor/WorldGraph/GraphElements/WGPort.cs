using System;
using UnityEditor.Experimental.GraphView;

namespace ThunderNut.SceneManagement.Editor {
    public class WGPort : Port {
        protected WGPort(Orientation portOrientation, Direction portDirection, Capacity portCapacity, Type type) : base(
            portOrientation, portDirection, portCapacity, type) {
        }
    }
}