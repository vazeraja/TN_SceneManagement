using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WGSearcherProvider : SearchWindowProvider {
        public Searcher LoadSearchWindow() {
            var bookTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "twotone_book_white_18dp" : "twotone_book_black_18dp");
            var scienceTexture = Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "twotone_science_white_18dp" : "twotone_science_black_18dp");
            var cookingTexture =
                Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "twotone_outdoor_grill_white_18dp" : "twotone_outdoor_grill_black_18dp");
            var namesTexture =
                Resources.Load<Texture2D>(EditorGUIUtility.isProSkin ? "twotone_emoji_people_white_18dp" : "twotone_emoji_people_black_18dp");


            var searchOptions = new List<SearcherItem> {
                new SearcherItem("Books", "", new List<SearcherItem> {
                    new SearchNodeItem("Cooking", userData: "Cooking Category", icon: cookingTexture),
                    new SearchNodeItem("Science Fiction", userData: "Science Fiction Category", icon: scienceTexture),
                    new SearchNodeItem("Names", userData: "Names Category", icon: namesTexture)
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

            if ((entry as SearchNodeItem)?.userData is "Names Category") {
                Debug.Log("Clicked Names Category");
                return true;
            }

            return true;
        }
    }
}