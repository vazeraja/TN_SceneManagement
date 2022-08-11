using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using UnityEditor.Searcher;
using UnityEditor.ShaderGraph;


namespace ThunderNut.SceneManagement.Editor {
    public class SearchWindowProvider : ScriptableObject {
        internal EditorWindow m_EditorWindow;
        internal GraphView m_GraphView;
        internal VisualElement target;
        internal Edge m_EdgeFilter;
        internal Port inputPort;
        internal Port outputPort;
        internal Texture2D m_Icon;

        public void Initialize(EditorWindow editorWindow, GraphView graphView, Edge edgeFilter = null) {
            m_EditorWindow = editorWindow;
            m_GraphView = graphView;
            m_EdgeFilter = edgeFilter;
            inputPort = edgeFilter?.input as Port;

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

    internal class SearchNodeItem : SearcherItem {
        public readonly string identifier;

        public SearchNodeItem(string name, string identifier = "", List<SearchNodeItem> newChildren = null,
            Texture2D newIcon = null)
            : base(name, icon: newIcon) {
            this.identifier = identifier;
        }
    }

    public class WGSearcherProvider : SearchWindowProvider {
        public Searcher LoadSearchWindow() {
            
            #region Setup Textures

            Texture2D bookTexture;
            Texture2D scienceTexture;
            Texture2D cookingTexture;
            Texture2D namesTexture;

            if (EditorGUIUtility.isProSkin) {
                bookTexture = Resources.Load<Texture2D>("twotone_book_white_18dp");
                scienceTexture = Resources.Load<Texture2D>("twotone_science_white_18dp");
                cookingTexture = Resources.Load<Texture2D>("twotone_outdoor_grill_white_18dp");
                namesTexture = Resources.Load<Texture2D>("twotone_emoji_people_white_18dp");
            }
            else {
                bookTexture = Resources.Load<Texture2D>("twotone_book_black_18dp");
                scienceTexture = Resources.Load<Texture2D>("twotone_science_black_18dp");
                cookingTexture = Resources.Load<Texture2D>("twotone_outdoor_grill_black_18dp");
                namesTexture = Resources.Load<Texture2D>("twotone_emoji_people_black_18dp");
            }

            #endregion

            var searchOptions = new List<SearcherItem> {
                new SearcherItem("Books", "", new List<SearcherItem> {
                    new SearchNodeItem("Cooking", "Cooking Category", newIcon: cookingTexture),
                    new SearchNodeItem("Science Fiction", "Science Fiction Category", newIcon: scienceTexture),
                    new SearchNodeItem("Names", "Names Category", newIcon: namesTexture)
                }, icon: bookTexture)
            };

            string databaseDir = Application.dataPath + "/../Library/Searcher";
            var nodeDatabase = SearcherDatabase.Create(searchOptions, databaseDir + "/WGNodeSearchOptions");
            return new Searcher(nodeDatabase, new SearchWindowAdapter("Create Node"));
        }

        public bool OnSearcherSelectEntry(SearcherItem entry, Vector2 screenMousePosition) {
            var windowRoot = m_EditorWindow.rootVisualElement;
            var windowMousePosition =
                windowRoot.ChangeCoordinatesTo(windowRoot.parent,
                    screenMousePosition); //- m_EditorWindow.position.position);
            var graphMousePosition = m_GraphView.contentViewContainer.WorldToLocal(windowMousePosition);

            if ((entry as SearchNodeItem)?.identifier == "Names Category") {
                Debug.Log("Clicked Names Category");
                return true;
            }

            return true;
        }
    }
}