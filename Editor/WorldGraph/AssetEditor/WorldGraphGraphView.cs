using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView {
        private readonly WorldGraph graph;
        
        private WorldGraphGraphView() { }

        public WorldGraphGraphView(WorldGraph graph) : this() {
            this.graph = graph;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            => ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node)
                .ToList();
    }

}