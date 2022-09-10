using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView {

        public WorldGraphGraphView() { }
     
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    ((WorldGraphPort)endPort).PortData.OwnerNodeGUID != ((WorldGraphPort)startPort).PortData.OwnerNodeGUID &&
                    ((WorldGraphPort)endPort).PortData.PortDirection != ((WorldGraphPort)startPort).PortData.PortDirection &&
                    ((WorldGraphPort)endPort).PortData.PortType == ((WorldGraphPort)startPort).PortData.PortType) 
                .ToList();
        }
    }

}