using System;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(WorldGraph))]
    public class WorldGraphEditor : UnityEditor.Editor {
        private StringListSearcherProvider stringListSearcherProvider;
        private Rect buttonRect;

        private void OnEnable() {
            stringListSearcherProvider = CreateInstance<StringListSearcherProvider>();
            stringListSearcherProvider.Initialize(WGHelpers.Ingredients,
                x => {
                    serializedObject.FindProperty("selectedItem").stringValue =
                        x ?? serializedObject.FindProperty("selectedItem").stringValue;
                });
        }

        public override void OnInspectorGUI() {
            if (GUILayout.Button("MultiColumnTreeView PopupWindow")) {
                PopupWindow.Show(buttonRect, new TreeViewPopupWindow {Width = buttonRect.width});
            }

            WGHelpers.HorizontalScope(() => {
                EditorGUILayout.LabelField("Selected Item");

                if (GUILayout.Button($"{serializedObject.FindProperty("selectedItem").stringValue}", EditorStyles.popup)) {
                    SearcherWindow.Show(EditorWindow.focusedWindow, stringListSearcherProvider.LoadSearchWindow(),
                        searcherItem => stringListSearcherProvider.OnSearcherSelectEntry(searcherItem),
                        EditorWindow.focusedWindow.rootVisualElement.LocalToWorld(Event.current.mousePosition), null);
                }
            });

            EditorGUILayout.PropertyField(serializedObject.FindProperty("DemoScriptableObject"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("SceneHandle"));

            if (Event.current.type == EventType.Repaint)
                buttonRect = GUILayoutUtility.GetLastRect();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            DestroyImmediate(stringListSearcherProvider);
            stringListSearcherProvider = null;
        }
    }

}