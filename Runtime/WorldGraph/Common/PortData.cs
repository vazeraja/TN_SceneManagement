using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    public enum PortType {
        Default,
        Parameter,
    }

    [Serializable]
    public class PortData {
        public string OwnerNodeGUID;
        public string GUID;

        public string PortDirection;
        public string PortCapacity;
        public PortType PortType;
        public Color PortColor;

        [SerializeReference] public ExposedParameter Parameter;
    }  

}