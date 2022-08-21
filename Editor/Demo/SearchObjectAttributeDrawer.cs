using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    [CustomPropertyDrawer(typeof(SearchObjectAttribute))]
    public class SearchObjectAttributeDrawer : PropertyDrawer {
        
        public static string[] Ingredients {
            get {
                return new[] {
                    "Car/Honda/Civic",
                    "Car/Honda/CRV",
                    "Car/BMW/328i",
                    "Electronics/Computer/Keyboard",
                    "Electronics/Computer/Mouse",
                    "Electronics/Computer/Monitor",
                    "Electronics/Computer/Headset",
                    "Electronics/Computer/Microphone",
                    "Meat/Chicken/Roasted",
                    "Meat/Chicken/Tenders",
                    "Meat/Chicken/Wings",
                    "Meat/Chicken/Legs",
                    "Meat/Chicken/Thigh",
                    "Meat/Chicken/Frozen",
                    "Meat/Pork/Joint",
                    "Meat/Pork/Slices",
                    "Meat/Pork/Shoulder",
                    "Meat/Pork/Assorted",
                    "Meat/Pork/Mixed",
                    "Meat/Sausages/Sliced",
                    "Meat/Sausages/Hotdogs",
                    "Meat/Sausages/Froze",
                    "Meat/Sausages/Butcher",
                    "Meat/Sausages/Pizza",
                    "Meat/Sausages/Italian",
                    "Meat/Turkey/Sliced",
                    "Meat/Turkey/Full",
                    "Meat/Turkey/Roasted",
                };
            }
        }
        
        private SerializedProperty serializedProperty;
        private EditorWindow editorWindow;
        private Type assetType;

        private static void BuildTree(IEnumerable<string> paths, out List<SearcherItem> result) {
            List<List<string>> entryItemsList = paths.Select(item => item.Split('/').ToList()).ToList();

            var root = new SearcherItem("Main");
            var current = root;

            foreach (var entryItems in entryItemsList) {
                for (var index = 0; index < entryItems.Count; index++) {
                    string entryItem = entryItems[index];
                    var match = current.Children.Find(x => x.Name == entryItem);

                    if (match == null) {
                        var temp = new SearcherItem(entryItem);

                        if (index == entryItems.Count - 1) {
                            string path = string.Join("/", entryItems);
                            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
                            temp = new SearchNodeItem(entryItem, userData: obj,
                                icon: (Texture2D) EditorGUIUtility.ObjectContent(obj, obj.GetType()).image);
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

        private Searcher LoadSearchWindow() {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{assetType.Name}");
            List<string> paths = assetGuids.Select(AssetDatabase.GUIDToAssetPath).ToList();
            BuildTree(paths, out var result);

            string databaseDir = Application.dataPath + "/../Library/Searcher";
            var nodeDatabase = SearcherDatabase.Create(result, databaseDir + "/Misc/RandomScriptableObjects");
            return new Searcher(nodeDatabase, new SearchWindowAdapter("Select Item"));
        }


        private bool OnSearcherSelectEntry(SearcherItem entry) {
            UnityEngine.Object userData = (UnityEngine.Object) (entry as SearchNodeItem)?.userData;

            serializedProperty.objectReferenceValue = userData ? userData : serializedProperty.objectReferenceValue;
            serializedProperty.serializedObject.ApplyModifiedProperties();
            return true;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            serializedProperty = property;
            assetType = property.GetPropertyAttribute<SearchObjectAttribute>(true).searchObjectType;

            position.width -= 60;
            EditorGUI.ObjectField(position, property, label);
            
            position.x += position.width;
            position.width = 60;
            if (GUI.Button(position, new GUIContent("Find"))) {
                SearcherWindow.Show(EditorWindow.focusedWindow, LoadSearchWindow(), OnSearcherSelectEntry,
                    EditorWindow.focusedWindow.rootVisualElement.LocalToWorld(Event.current.mousePosition), null);
            }
        }
    }

}