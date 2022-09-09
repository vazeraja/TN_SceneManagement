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

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    // endPort.direction != startPort.direction &&
                    ((WorldGraphPort)endPort).PortData.PortDirection != ((WorldGraphPort)startPort).PortData.PortDirection &&
                    endPort.node != startPort.node &&
                    ((WorldGraphPort)endPort).PortData.PortType == ((WorldGraphPort)startPort).PortData.PortType) 
                .ToList();
        }
    }

}