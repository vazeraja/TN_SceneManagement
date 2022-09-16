using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;
using UnityEngine;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    public class TemporaryPopupWindow : PopupWindowContent {
        private readonly SearchField m_SearchField;

        private WGSimpleTreeView multiColumnTreeView;
        private TreeViewState multiColumnTreeViewState;
        private MultiColumnHeaderState multiColumnHeaderState;

        private bool m_ShouldClose;
        public float Width;

        public TemporaryPopupWindow(List<ExposedParameter> parameters, SerializedProperty property) {
            m_SearchField = new SearchField();
            multiColumnTreeView = WGSimpleTreeView.Create(ref multiColumnTreeViewState, ref multiColumnHeaderState, parameters);
            multiColumnTreeView.onDoubleClicked = parameter => {
                property.objectReferenceValue = parameter;
                property.serializedObject.ApplyModifiedProperties();
                ForceClose();
            };
        }

        public override void OnGUI(Rect rect) {
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2,
                rect.height - remainTop - border);

            multiColumnTreeView.searchString = m_SearchField.OnGUI(searchRect, multiColumnTreeView.searchString);
            multiColumnTreeView.OnGUI(remainingRect);
        }

        public override Vector2 GetWindowSize() {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen() {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public override void OnClose() {
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

        private SerializedProperty childrenProperty;
        private ReorderableList childrenReorderableList;

        private void OnEnable() {
            sceneHandle = target as SceneHandle;
            transitionsProperty = serializedObject.FindProperty("transitions");
            childrenProperty = serializedObject.FindProperty("children");

            childrenReorderableList = new ReorderableList(serializedObject, childrenProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                drawHeaderCallback = rect => EditorGUI.LabelField(rect, childrenProperty.displayName),
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(childrenProperty.GetArrayElementAtIndex(index)),
                drawElementCallback = (rect, index, active, focused) => {
                    var element = childrenProperty.GetArrayElementAtIndex(index);
                    EditorGUI.PropertyField(rect, element, new GUIContent("SceneHandle"));
                }
            };

            transitionsReorderableList = new ReorderableList(serializedObject, transitionsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                drawHeaderCallback = rect => EditorGUI.LabelField(rect, transitionsProperty.displayName),
                elementHeightCallback = index => {
                    var element = transitionsProperty.GetArrayElementAtIndex(index);
                    var outputProp = element.FindPropertyRelative("Parameter");
                    return EditorGUI.GetPropertyHeight(outputProp);
                },
                drawElementCallback = (rect, index, active, focused) => {
                    var element = transitionsProperty.GetArrayElementAtIndex(index);
                    var parameterProp = element.FindPropertyRelative("Parameter");

                    if (sceneHandle.allParameters.Any()) {
                        float width = rect.width / 2;
                        rect.width = width;
                        
                        List<ExposedParameter> allParams = sceneHandle.allParameters.ToList();

                        if (EditorGUI.DropdownButton(rect, parameterProp.objectReferenceValue != null
                                ? new GUIContent(((ExposedParameter) parameterProp.objectReferenceValue).Name)
                                : new GUIContent("Select a Parameter"),
                            FocusType.Passive)) {
                            PopupWindow.Show(rect, new TemporaryPopupWindow(allParams, parameterProp) {Width = rect.width});
                        }

                        rect.x += width + 5;
                        rect.width = width / 2 - 5;

                        switch (parameterProp.objectReferenceValue) {
                            case StringParameterField stringParameterField:
                                stringParameterField.options =
                                    (StringParamOptions) EditorGUI.EnumPopup(rect, stringParameterField.options);
                                break;
                            case FloatParameterField floatParameterField:
                                floatParameterField.options =
                                    (FloatParamOptions) EditorGUI.EnumPopup(rect, floatParameterField.options);
                                break;
                            case IntParameterField intParameterField:
                                intParameterField.options =
                                    (IntParamOptions) EditorGUI.EnumPopup(rect, intParameterField.options);
                                break;
                            case BoolParameterField boolParameterField:
                                boolParameterField.options =
                                    (BoolParamOptions) EditorGUI.EnumPopup(rect, boolParameterField.options);
                                break;
                        }
                        
                        rect.x += width / 2;
                        EditorGUI.TextField(rect, "Label");
                    }
                    else {
                        EditorGUI.HelpBox(rect, "This SceneHandle has no connected parameters", MessageType.Warning);
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
            childrenReorderableList.DoLayoutList();
            transitionsReorderableList.DoLayoutList();
            if (sceneHandle.transitions.First().Parameter != null)
                Debug.Log(sceneHandle.transitions.First().Parameter.Name);

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floatParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boolParameters"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}