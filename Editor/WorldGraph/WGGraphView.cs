using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WGGraphView : GraphView {
        private readonly WorldGraph graph;

        public WGGraphView() { }

        public WGGraphView(WorldGraph graph) : this() {
            this.graph = graph;
            this.graphViewChanged = null;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            => ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node)
                .ToList();
    }

}