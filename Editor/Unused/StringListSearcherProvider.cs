﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEditor.ShaderGraph;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class NodeEntry {
        public string name = "default";
        public List<NodeEntry> children;

        public NodeEntry() {
            children = new List<NodeEntry>();
        }

        public NodeEntry(string name) : this() {
            this.name = name;
        }
    }

    public class StringListSearcherProvider : SearchWindowProvider {
        private static void BuildTree(IEnumerable<List<string>> entryItemsList, out List<SearcherItem> result) {
            var root = new SearcherItem("Main");
            var current = root;

            foreach (var entryItems in entryItemsList) {
                for (var index = 0; index < entryItems.Count; index++) {
                    string entryItem = entryItems[index];
                    var match = current.Children.Find(x => x.Name == entryItem);

                    if (match == null) {
                        var temp = new SearcherItem(entryItem);

                        if (index == entryItems.Count - 1) {
                            temp = new SearchNodeItem(entryItem, userData: entryItem);
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
            var sortedListItems = listItems.ToList();
            sortedListItems.Sort((entry1, entry2) => {
                string[] splits1 = entry1.Split('/');
                string[] splits2 = entry2.Split('/');
                for (var i = 0; i < splits1.Length; i++) {
                    if (i >= splits2.Length)
                        return 1;
                    int value = string.Compare(splits1[i], splits2[i], StringComparison.Ordinal);
                    if (value == 0) continue;
                    // Make sure that leaves go before nodes
                    if (splits1.Length == splits2.Length || (i != splits1.Length - 1 && i != splits2.Length - 1)) return value;
                    int alphaOrder = splits1.Length < splits2.Length ? -1 : 1;
                    return alphaOrder;
                }

                return 0;
            });
            
            var tree = sortedListItems.Select(item => item.Split('/').ToList()).ToList();
            BuildTree(tree, out var result);

            string databaseDir = Application.dataPath + "/../Library/Searcher";
            var nodeDatabase = SearcherDatabase.Create(result, databaseDir + "/Misc/RandomStrings");
            return new Searcher(nodeDatabase, new SearchWindowAdapter("Select Item"));
        }

        public bool OnSearcherSelectEntry(SearcherItem entry) {
            if ((entry as SearchNodeItem)?.userData is "Civic") {
                Debug.Log("Clicked 'Civic' item");
                return true;
            }

            return true;
        }
    }
}