using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    public abstract class SceneHandle : ScriptableObject {
        public string GUID;
        public Vector2 Position;
        public List<PortData> Ports = new List<PortData>();
        protected virtual Color HandleColor => Color.white;
        public Color Color => HandleColor;

        [WGInspectable] public WorldGraph WorldGraph;
        [WGInspectable] public bool Active = true;
        [WGInspectable] public string HandleName = "";
        [WGInspectable] public SceneReference scene;
        public List<SceneHandle> children = new List<SceneHandle>();
        
        public void Enter() { }
        public void Exit() { }

        public abstract void ChangeToScene();

        public PortData CreatePort(string ownerGUID, bool isOutput, bool isMulti, bool isParameter, Color portColor) {
            var portData = new PortData {
                OwnerNodeGUID = ownerGUID,
                GUID = Guid.NewGuid().ToString(),

                PortDirection = isOutput ? "Output" : "Input",
                PortCapacity = isMulti ? "Multi" : "Single",
                PortType = isParameter ? PortType.Parameter : PortType.Default,
                PortColor = portColor,
            };
            Ports.Add(portData);
            return portData;
        }

        public void RemovePort(PortData portData) {
            Ports.Remove(portData);
        }
    }

}