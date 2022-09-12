using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphGraphView : GraphView {

        public WorldGraphGraphView() {
            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGGraphView"));
            
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ClickSelector());
            
            SetupZoom(0.05f, 8);
            AddToClassList("drop-area");
        }
     
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter) {
            return ports.ToList().Where(endPort =>
                    ((WorldGraphPort)endPort).PortData.OwnerNodeGUID != ((WorldGraphPort)startPort).PortData.OwnerNodeGUID &&
                    ((WorldGraphPort)endPort).PortData.PortDirection != ((WorldGraphPort)startPort).PortData.PortDirection &&
                    ((WorldGraphPort)endPort).PortData.PortType == ((WorldGraphPort)startPort).PortData.PortType) 
                .ToList();
        }

        public override void AddToSelection(ISelectable selectable) {
            base.AddToSelection(selectable);
        }
    }

}