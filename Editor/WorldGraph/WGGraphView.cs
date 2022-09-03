using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WGGraphView : GraphView {
        private readonly WorldGraph graph;

        public Blackboard blackboard = new Blackboard();

        public WGGraphView() { }

        public WGGraphView(WorldGraph graph) : this() {
            this.graph = graph;

            blackboard = new Blackboard(this) {title = "WorldGraph"};
            blackboard.Add(new BlackboardSection {
                title = "Exposed Properties"
            });

            blackboard.addItemRequested += blackboard1 => { Debug.Log("item requested"); };

            Add(blackboard);
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
            => ports.ToList().Where(endPort => endPort.direction != startPort.direction && endPort.node != startPort.node)
                .ToList();
    }

}