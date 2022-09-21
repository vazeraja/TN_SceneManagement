// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEditor;
// using UnityEditorInternal;
// using UnityEngine;
// using Debug = System.Diagnostics.Debug;
//
// namespace ThunderNut.SceneManagement.Editor {
//
//     [CustomEditor(typeof(Transition))]
//     public class TransitionEditor : UnityEditor.Editor {
//         private Transition transition;
//
//         private SerializedProperty conditionsProperty;
//         private ReorderableList conditionsReorderableList;
//
//         public override void OnInspectorGUI() {
//             serializedObject.Update();
//
//             EditorGUILayout.HelpBox(new GUIContent("Select Conditions for this Transition"));
//             conditionsReorderableList.DoLayoutList();
//             EditorGUILayout.Separator();
//             EditorGUILayout.PropertyField(serializedObject.FindProperty("Conditions"));
//
//             serializedObject.ApplyModifiedProperties();
//         }
//
//         private void OnEnable() {
//             transition = target as Transition;
//             conditionsProperty = serializedObject.FindProperty("Conditions");
//
//             conditionsReorderableList = new ReorderableList(serializedObject, conditionsProperty) {
//                 displayAdd = true,
//                 displayRemove = true,
//                 draggable = true,
//
//                 drawHeaderCallback = rect => EditorGUI.LabelField(rect, conditionsProperty.displayName),
//                 elementHeightCallback = index => {
//                     var element = conditionsProperty.GetArrayElementAtIndex(index);
//                     var prop = element.FindPropertyRelative("Value");
//                     return EditorGUIUtility.singleLineHeight;
//                 },
//                 drawElementCallback = (rect, index, active, focused) => {
//                     var element = conditionsProperty.GetArrayElementAtIndex(index);
//                     var parameterProp = element.FindPropertyRelative("Parameter");
//                     var valueProp = element.FindPropertyRelative("Value");
//
//                     // var valueObj = valueProp.managedReferenceValue as ConditionValueBase;
//
//                     // var stringValue = valueProp.FindPropertyRelative("StringValue").stringValue;
//                     // var floatValue = valueProp.FindPropertyRelative("FloatValue").floatValue;
//                     // var intValue = valueProp.FindPropertyRelative("IntValue").intValue;
//
//                     float width = rect.width / 2;
//
//                     SceneHandle sceneHandle = transition.OutputNode;
//                     var allParameters = transition.WorldGraph.ExposedParameterViewDatas
//                         .FindAll(data => data.connectedNode == sceneHandle)
//                         .Select(data => data.parameter)
//                         .ToList();
//
//                     if (sceneHandle != null && allParameters.Any()) {
//                         rect.width = width;
//
//                         if (EditorGUI.DropdownButton(rect, parameterProp.managedReferenceValue != null
//                             ? new GUIContent(((ExposedParameter) parameterProp.managedReferenceValue).Name)
//                             : new GUIContent("Select a Parameter"), FocusType.Passive)) {
//                             PopupWindow.Show(rect,
//                                 new ConditionOptionsPopupWindow(allParameters, parameterProp, valueProp) {Width = rect.width});
//                         }
//
//                         rect.x += width + 5;
//                         rect.width = width / 2 - 5;
//
//                         switch (parameterProp.managedReferenceValue) {
//                             case StringParameterField:
//
//                                 if (valueProp.managedReferenceValue is StringCondition stringCondition) {
//                                     stringCondition.stringOptions =
//                                         (StringParamOptions) EditorGUI.EnumPopup(rect, stringCondition.stringOptions);
//
//                                     rect.x += width / 2;
//                                     stringCondition.Value =
//                                         EditorGUI.TextField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5), GUIContent.none,
//                                             stringCondition.Value);
//                                 }
//
//                                 break;
//                             case FloatParameterField:
//                                 if (valueProp.managedReferenceValue is FloatCondition floatCondition) {
//                                     floatCondition.floatOptions =
//                                         (FloatParamOptions) EditorGUI.EnumPopup(rect, floatCondition.floatOptions);
//
//                                     rect.x += width / 2;
//                                     floatCondition.Value =
//                                         EditorGUI.FloatField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
//                                             GUIContent.none, floatCondition.Value);
//                                 }
//
//                                 break;
//                             case IntParameterField:
//                                 if (valueProp.managedReferenceValue is IntCondition intCondition) {
//                                     intCondition.intOptions = (IntParamOptions) EditorGUI.EnumPopup(rect, intCondition.intOptions);
//
//                                     rect.x += width / 2;
//                                     intCondition.Value =
//                                         EditorGUI.IntField(new Rect(rect.x, rect.y + 1, rect.width, rect.height - 5),
//                                             GUIContent.none, intCondition.Value);
//                                 }
//
//                                 break;
//                             case BoolParameterField:
//                                 if (valueProp.managedReferenceValue is BoolCondition boolCondition) {
//                                     boolCondition.boolOptions =
//                                         (BoolParamOptions) EditorGUI.EnumPopup(rect, boolCondition.boolOptions);
//                                     boolCondition.Value = boolCondition.boolOptions switch {
//                                         BoolParamOptions.True => true,
//                                         BoolParamOptions.False => false,
//                                         _ => boolCondition.Value
//                                     };
//                                 }
//
//
//                                 break;
//                         }
//                     }
//                     else {
//                         EditorGUI.HelpBox(rect, "This SceneHandle has no connected parameters", MessageType.Warning);
//                     }
//                 }
//             };
//         }
//     }
//
// }