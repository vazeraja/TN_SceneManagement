using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class SearchNodeItem : SearcherItem {
        public readonly Type type;
        public readonly object userData;

        public SearchNodeItem(string name, Type type = null, object userData = null,
            List<SearchNodeItem> newChildren = null, Texture2D icon = null) :
            base(name, userData: userData, icon: icon) {
            this.type = type;
            this.userData = userData;
        }
    }

    public class WorldGraphSearcherProvider : ScriptableObject {
        private EditorWindow editorWindow;
        private GraphView graphView;
        private Action<Type, Vector2> itemSelectedCallback;
        public VisualElement target;

        public void Initialize(EditorWindow editorWindow, GraphView graphView = null, Action<Type, Vector2> itemSelectedCallback = null) {
            this.editorWindow = editorWindow;
            this.graphView = graphView;
            this.itemSelectedCallback = itemSelectedCallback;
        }

        /// <summary>
        /// Convert a list of string lists into a SearcherItem tree structure
        /// </summary>
        /// <param name="entryItemsList"></param>
        /// <param name="result"></param>
        private static void BuildTree(IEnumerable<List<string>> entryItemsList, out List<SearcherItem> result) {
            var root = new SearcherItem("Main");
            var current = root;

            foreach (var entryItems in entryItemsList) {
                var t = WGAttributeCache.GetTypeByName(entryItems.Last());
                var attribute = WGAttributeCache.GetAttributeOnNodeType<PathAttribute>(t);
                if (!attribute.isVisible) continue;
                for (var index = 0; index < entryItems.Count; index++) {
                    string entryItem = entryItems[index];
                    var match = current.Children.Find(x => x.Name == entryItem);

                    if (match == null) {
                        var temp = new SearcherItem(entryItem);

                        if (index == entryItems.Count - 1) {
                            // Get the type associated with the leaf item
                            // var type = WGAttributeCache.knownNodeTypes.ToList().Find(x => x.Name == entryItem);
                            // Debug.Log(type.AssemblyQualifiedName);
                            // Get dropdown title from attribute
                            // var type = Type.GetType(entryItem);

                            var type = WGAttributeCache.GetTypeByName(entryItem);
                            var attr = WGAttributeCache.GetAttributeOnNodeType<PathAttribute>(type);

                            temp = new SearchNodeItem(attr.dropdownTitle, type, userData: type);
                        }

                        current.AddChild(temp);
                        current = temp;
                    }
                    else {
                        current = match;
                    }
                }

                current = root;
            }

            result = root.Children.Select(child => new SearcherItem(child.Name, children: child.Children)).ToList();
        }

        public Searcher LoadSearchWindow() {
            var sortedListItems = WGAttributeCache.GetSortedNodePathsList();
            List<List<string>> tree = sortedListItems.Select(item => item.Split('/').ToList()).ToList();
            BuildTree(tree, out var result);

            string databaseDir = Application.dataPath + "/../Library/Searcher";
            var nodeDatabase = SearcherDatabase.Create(result, databaseDir + "/WGNodeSearchOptions");
            return new Searcher(nodeDatabase, new SearchWindowAdapter("Create Node"));
        }

        public bool OnSearcherSelectEntry(SearcherItem entry, Vector2 screenMousePosition) {
            var windowRoot = editorWindow.rootVisualElement;
            var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot.parent, screenMousePosition);
            var graphMousePosition = graphView.contentViewContainer.WorldToLocal(windowMousePosition);

            var selectedEntryType = ((SearchNodeItem) entry)?.type;

            // ReSharper disable once InvertIf
            if (selectedEntryType != null && selectedEntryType.IsSubclassOf(typeof(SceneHandle))) {
                itemSelectedCallback?.Invoke(((SearchNodeItem) entry).type, graphMousePosition);
                return true;
            }

            return false;
        }
    }

}