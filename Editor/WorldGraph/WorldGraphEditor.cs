using System;
using UnityEditor;
using UnityEditor.Searcher;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(WorldGraph), true)]
    public class WorldGraphEditor : UnityEditor.Editor {
        private Rect buttonRect;

        public SerializedObject worldGraphSerializedObject;

        private SerializedProperty selectedItemProp;
        private SerializedProperty demoScriptableObjectProp;
        private SerializedProperty sceneHandleProp;

        private SceneHandle handle;

        private bool _settingsMenuDropdown;

        private void OnEnable() {
            demoScriptableObjectProp = serializedObject.FindProperty("DemoScriptableObject");
            sceneHandleProp = serializedObject.FindProperty("SceneHandle");

            handle = sceneHandleProp.objectReferenceValue as SceneHandle;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            if (GUILayout.Button("MultiColumnTreeView PopupWindow")) {
                PopupWindow.Show(buttonRect, new TreeViewPopupWindow {Width = buttonRect.width});
            }
            
            WGHelpers.DrawSection("Yeehaw");
            WGHelpers.DrawSimpleHeader(ref _settingsMenuDropdown, ref handle.Active, "Settings");

            if (_settingsMenuDropdown) {
                EditorGUILayout.PropertyField(demoScriptableObjectProp);
                EditorGUILayout.PropertyField(sceneHandleProp);
            }

            if (Event.current.type == EventType.Repaint)
                buttonRect = GUILayoutUtility.GetLastRect();

            if (serializedObject.ApplyModifiedProperties()) {
                Debug.Log("something changed");
            }
        }
    }
}