using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class ExposedParameter {
        [SerializeField] private string m_guid;
        public string GUID {
            get => m_guid;
            set => m_guid = value;
        }
        
        [SerializeField] private string m_Name;
        public string Name {
            get => m_Name;
            set => m_Name = value;
        }

        [SerializeField] private string m_Reference;
        public string Reference {
            get => m_Reference;
            set => m_Reference = value;
        }

        [SerializeField] private bool m_Exposed;
        public bool Exposed {
            get => m_Exposed;
            set => m_Exposed = value;
        }
        [SerializeField] private bool m_Displayed;
        public bool Displayed {
            get => m_Displayed;
            set => m_Displayed = value;
        }
        
        [SerializeField] private string m_ConnectedPortGUID;
        public string ConnectedPortGUID {
            get => m_ConnectedPortGUID;
            set => m_ConnectedPortGUID = value;
        }
        
        public Vector2 position;

        [SerializeField] private ParameterType m_ParameterType;
        public ParameterType ParameterType {
            get => m_ParameterType;
            protected set => m_ParameterType = value;
        }
    }

}