using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    [CustomEditor(typeof(MyDemoObject))]
    public class MyDemoObjectEditor : UnityEditor.Editor {
        public static string[] Ingredients {
            get {
                return new[] {
                    "Car/Honda/Civic",
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

        private EditorWindow inspectorWindow;
        private StringListSearcherProvider searchProvider;
        private Rect buttonRect;

        private void OnEnable() {
            inspectorWindow = Resources.FindObjectsOfTypeAll<EditorWindow>().ToList()
                .Find(x => x.titleContent.ToString() == "Inspector");
            searchProvider = CreateInstance<StringListSearcherProvider>();
            searchProvider.Initialize(inspectorWindow, items: Ingredients, callback: (x) => {
                ((MyDemoObject) target).selectedItem = x;
            });
        }

        public override void OnInspectorGUI() {
            MyDemoObject item = (MyDemoObject) target;

            WGEditor.HorizontalScope(() => {
                EditorGUILayout.LabelField("Selected Item", GUILayout.ExpandWidth(false), GUILayout.Width(250));

                if (GUILayout.Button($"{item.selectedItem}", EditorStyles.popup)) {
                    var localMousePosition = Event.current.mousePosition;
                    var worldMousePosition = inspectorWindow.rootVisualElement.LocalToWorld(localMousePosition);
                    SearcherWindow.Show(inspectorWindow, searchProvider.LoadSearchWindow(),
                        searcherItem => searchProvider.OnSearcherSelectEntry(searcherItem),
                        worldMousePosition, null);
                }

                if (Event.current.type == EventType.Repaint)
                    buttonRect = GUILayoutUtility.GetLastRect();
            });

            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetObject"));
            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            DestroyImmediate(searchProvider);
            searchProvider = null;
        }
    }
}