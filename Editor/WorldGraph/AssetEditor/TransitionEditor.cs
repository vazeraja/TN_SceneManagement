using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(Transition))]
    public class TransitionEditor : UnityEditor.Editor {
        private Transition transition;

        private SerializedProperty conditionsProperty;
        private ReorderableList conditionsReorderableList;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.HelpBox(new GUIContent("Select Conditions for this Transition"));
            conditionsReorderableList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            transition = target as Transition;
            conditionsProperty = serializedObject.FindProperty("Conditions");

            conditionsReorderableList = new ReorderableList(serializedObject, conditionsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                drawHeaderCallback = rect => EditorGUI.LabelField(rect, conditionsProperty.displayName),
                elementHeightCallback = index => {
                    var element = conditionsProperty.GetArrayElementAtIndex(index);
                    var outputProp = element.FindPropertyRelative("Parameter");
                    return EditorGUI.GetPropertyHeight(outputProp);
                },
                drawElementCallback = (rect, index, active, focused) => {
                    var element = conditionsProperty.GetArrayElementAtIndex(index);
                    var parameterProp = element.FindPropertyRelative("Parameter");
                    var valueProp = element.FindPropertyRelative("Value");

                    var stringValue = valueProp.FindPropertyRelative("StringValue").stringValue;
                    var floatValue = valueProp.FindPropertyRelative("FloatValue").floatValue;
                    var intValue = valueProp.FindPropertyRelative("IntValue").intValue;
                    var boolValue = valueProp.FindPropertyRelative("BoolValue").boolValue;

                    float width = rect.width / 2;

                    SceneHandle sceneHandle = transition.OutputNode;

                    if (sceneHandle != null && sceneHandle.allParameters.Any()) {
                        rect.width = width;

                        List<ExposedParameter> allParams = sceneHandle.allParameters.ToList();

                        if (EditorGUI.DropdownButton(rect, parameterProp.objectReferenceValue != null
                            ? new GUIContent(((ExposedParameter) parameterProp.objectReferenceValue).Name)
                            : new GUIContent("Select a Parameter"), FocusType.Passive)) {
                            PopupWindow.Show(rect, new ConditionOptionsPopupWindow(allParams, parameterProp) {Width = rect.width});
                        }

                        rect.x += width + 5;
                        rect.width = width / 2 - 5;

                        switch (parameterProp.objectReferenceValue) {
                            case StringParameterField stringParameterField:
                                stringParameterField.options =
                                    (StringParamOptions) EditorGUI.EnumPopup(rect, stringParameterField.options);

                                rect.x += width / 2;
                                valueProp.FindPropertyRelative("StringValue").stringValue =
                                    EditorGUI.TextField(rect, GUIContent.none, stringValue);

                                break;
                            case FloatParameterField floatParameterField:
                                floatParameterField.options =
                                    (FloatParamOptions) EditorGUI.EnumPopup(rect, floatParameterField.options);

                                rect.x += width / 2;
                                valueProp.FindPropertyRelative("FloatValue").floatValue =
                                    EditorGUI.FloatField(rect, GUIContent.none, floatValue);

                                break;
                            case IntParameterField intParameterField:
                                intParameterField.options =
                                    (IntParamOptions) EditorGUI.EnumPopup(rect, intParameterField.options);

                                rect.x += width / 2;
                                valueProp.FindPropertyRelative("IntValue").intValue =
                                    EditorGUI.IntField(rect, GUIContent.none, intValue);

                                break;
                            case BoolParameterField boolParameterField:
                                boolParameterField.options =
                                    (BoolParamOptions) EditorGUI.EnumPopup(rect, boolParameterField.options);

                                rect.x += width / 2;

                                valueProp.FindPropertyRelative("BoolValue").boolValue = boolParameterField.options switch {
                                    BoolParamOptions.True => true,
                                    BoolParamOptions.False => false,
                                    _ => valueProp.FindPropertyRelative("BoolValue").boolValue
                                };

                                break;
                        }
                    }
                    else {
                        EditorGUI.HelpBox(rect, "This SceneHandle has no connected parameters", MessageType.Warning);
                    }
                }
            };
        }
    }

}