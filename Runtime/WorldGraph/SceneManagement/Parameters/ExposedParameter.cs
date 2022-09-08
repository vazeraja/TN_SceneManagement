using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement {

    [Serializable]
    public class ExposedParameter {
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

        [SerializeField] private ParameterType m_ParameterType;
        public ParameterType ParameterType {
            get => m_ParameterType;
            protected set => m_ParameterType = value;
        }
    }

}