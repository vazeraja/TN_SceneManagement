using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    public class TemporaryPopupWindow : PopupWindowContent {
        private bool m_ShouldClose;
        
        public float Width;
        public float Height;

        public override void OnGUI(Rect rect) {
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }
        }
        
        public override Vector2 GetWindowSize() {
            var result = base.GetWindowSize();
            result.x = Width;
            result.y = Height;
            return result;
        }

        public override void OnOpen() {
            base.OnOpen();
        }

        public override void OnClose(){
            base.OnClose();
        }

        public void ForceClose() => m_ShouldClose = true;
    }

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        private bool _settingsDropdown;
        private Rect buttonRect;

        private SceneHandle sceneHandle;

        private SerializedProperty transitionsProperty;
        private ReorderableList transitionsReorderableList;

        private static class Styles {
            public static readonly GUIContent RemoveIcon = EditorGUIUtility.IconContent("d_tab_next");
            public static readonly GUIStyle IconButton = new GUIStyle("IconButton");
        }

        private void OnEnable() {
            sceneHandle = target as SceneHandle;

            transitionsProperty = serializedObject.FindProperty("transitions");
            transitionsReorderableList = new ReorderableList(serializedObject, transitionsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                drawHeaderCallback = rect => EditorGUI.LabelField(rect, transitionsProperty.displayName),
                elementHeightCallback = index => {
                    var element = transitionsProperty.GetArrayElementAtIndex(index);
                    var outputProp = element.FindPropertyRelative("Output");
                    return EditorGUI.GetPropertyHeight(outputProp);
                },
                drawElementCallback = (rect, index, active, focused) => {
                    var element = transitionsProperty.GetArrayElementAtIndex(index);
                    var parameterProp = element.FindPropertyRelative("Parameter");
                    var outputProp = element.FindPropertyRelative("Output");
                    var inputProp = element.FindPropertyRelative("Input");

                    float width = rect.width / 2;
                    rect.width = width;

                    if (sceneHandle.allParameters.Count() != 0) {
                        List<ExposedParameter> allParams = sceneHandle.allParameters.ToList();
                        GUIContent[] dropdownOptions = allParams.Select(item => new GUIContent(item.Name)).ToArray();

                        if (EditorGUI.DropdownButton(rect, dropdownOptions.First(), FocusType.Passive)) {
                            PopupWindow.Show(new Rect(buttonRect.x, buttonRect.y + 10, buttonRect.width, buttonRect.height),
                                new TemporaryPopupWindow {Width = buttonRect.width, Height = 300});
                        }
                        
                        if (Event.current.type == EventType.Repaint) buttonRect = GUILayoutUtility.GetLastRect();

                        rect.x += width + 5;
                        rect.width = width / 2 - 5;

                        if (parameterProp.objectReferenceValue is FloatParameterField floatParameterField) {
                            floatParameterField.options =
                                (FloatParamOptions) EditorGUI.EnumPopup(rect, floatParameterField.options);
                        }

                        // EditorGUI.PropertyField(rect, outputProp, GUIContent.none);
                        rect.x += width / 2;
                        EditorGUI.PropertyField(rect, inputProp, GUIContent.none);
                    }
                }
            };
        }

        public override void OnInspectorGUI() {
            DrawProperties();
        }

        private void DrawProperties() {
            serializedObject.Update();

            using (new EditorGUI.DisabledGroupScope(true)) {
                _settingsDropdown = EditorGUILayout.Foldout(_settingsDropdown, "Internal Settings", true, EditorStyles.foldout);
                if (_settingsDropdown) {
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("GUID"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Position"));
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("Ports"));
                }
            }

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("WorldGraph"));
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("HandleName"));

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("children"));
            transitionsReorderableList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floatParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boolParameters"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}