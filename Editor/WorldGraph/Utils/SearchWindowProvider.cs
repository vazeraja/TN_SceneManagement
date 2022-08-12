using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.Searcher;


namespace ThunderNut.SceneManagement.Editor {
    public class SearchWindowProvider : ScriptableObject {
        internal EditorWindow m_EditorWindow;
        internal GraphView m_GraphView;
        internal VisualElement target;
        internal Texture2D m_Icon;
        internal string[] listItems;

        public void Initialize(EditorWindow editorWindow, GraphView graphView = null, string[] items = null) {
            m_EditorWindow = editorWindow;
            m_GraphView = graphView;
            listItems = items;

            // Transparent icon to trick search window into indenting items
            m_Icon = new Texture2D(1, 1);
            m_Icon.SetPixel(0, 0, new Color(0, 0, 0, 0));
            m_Icon.Apply();
        }

        private void OnDestroy() {
            if (m_Icon == null) return;
            DestroyImmediate(m_Icon);
            m_Icon = null;
        }
    }

    public class SearchNodeItem : SearcherItem {
        public readonly string identifier;
        public readonly object userData;

        public SearchNodeItem(string name, string identifier = "", object userData = null,
            List<SearchNodeItem> newChildren = null, Texture2D icon = null) : base(name, userData: userData, icon: icon) {
            this.identifier = identifier;
            this.userData = userData;
        }
    }
}