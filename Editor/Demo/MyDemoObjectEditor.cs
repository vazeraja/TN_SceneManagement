using System;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(MyDemoObject))]
    public class MyDemoObjectEditor : UnityEditor.Editor {
        private EditorWindow inspectorWindow;
        private StringListSearcherProvider searchProvider;
        private Rect buttonRect;
        private MyDemoObject item;
        
        [NonSerialized] bool m_Initialized;
        [SerializeField] TreeViewState m_TreeViewState; // Serialized in the window layout file so it survives assembly reloading
        [SerializeField] MultiColumnHeaderState m_MultiColumnHeaderState;
        SearchField m_SearchField;
        MultiColumnTreeView m_TreeView;

        private void OnEnable() {
            item = (MyDemoObject) target;
            
            inspectorWindow = WGHelpers.GetEditorWindowByName("Inspector");
            searchProvider = CreateInstance<StringListSearcherProvider>();
            searchProvider.Initialize(WGHelpers.Ingredients, x => { item.selectedItem = x ?? item.selectedItem; });
            
            
        }

        public override void OnInspectorGUI() {
            // Searcher Window Example 
            WGHelpers.HorizontalScope(() => {
                EditorGUILayout.LabelField("Selected Item", GUILayout.ExpandWidth(false), GUILayout.Width(250));

                if (GUILayout.Button($"{item.selectedItem}", EditorStyles.popup)) {
                    SearcherWindow.Show(inspectorWindow, searchProvider.LoadSearchWindow(),
                        searcherItem => searchProvider.OnSearcherSelectEntry(searcherItem),
                        inspectorWindow.rootVisualElement.LocalToWorld(Event.current.mousePosition), null,
                        new SearcherWindow.Alignment(SearcherWindow.Alignment.Vertical.Top,
                            SearcherWindow.Alignment.Horizontal.Right));
                }
            });

            // Custom Property Drawer Example
            EditorGUILayout.PropertyField(serializedObject.FindProperty("targetObject"));

            // TreeView PopupWindow Example
            
            
            
            WGHelpers.HorizontalScope(() => {
                GUILayout.Label("Editor window with popup", EditorStyles.boldLabel);
                if (GUILayout.Button("Popup Window")) {
                    PopupWindow.Show(buttonRect, new TreeViewPopupWindow(new WGSimpleTreeView(), buttonRect.width));
                }

                if (Event.current.type == EventType.Repaint)
                    buttonRect = GUILayoutUtility.GetLastRect();
            });


            serializedObject.ApplyModifiedProperties();
        }

        private void OnDisable() {
            DestroyImmediate(searchProvider);
            searchProvider = null;
        }
    }

}