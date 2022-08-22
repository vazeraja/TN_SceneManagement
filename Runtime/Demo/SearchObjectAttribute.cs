using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Method | AttributeTargets.Class)]
    public class SearchObjectAttribute : PropertyAttribute {
        public readonly Type searchObjectType;

        public SearchObjectAttribute(Type searchObjectType) {
            this.searchObjectType = searchObjectType;
        }
    }
}