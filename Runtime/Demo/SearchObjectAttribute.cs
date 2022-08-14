using System;
using UnityEngine;

namespace ThunderNut.SceneManagement {
    public class SearchObjectAttribute : PropertyAttribute {
        public readonly Type searchObjectType;

        public SearchObjectAttribute(Type searchObjectType) {
            this.searchObjectType = searchObjectType;
        }
    }
}