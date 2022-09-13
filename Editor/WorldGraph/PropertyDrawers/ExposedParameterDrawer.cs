// using System;
// using System.Linq;
// using UnityEditor;
// using UnityEngine;
//
// namespace ThunderNut.SceneManagement.Editor {
//
//     [CustomPropertyDrawer(typeof(ExposedParameterAttribute), true)]
//     public class ExposedParameterDrawer : PropertyDrawer {
//         public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
//             position.width -= 60;
//             EditorGUI.PropertyField(position, property, label);
//
//             position.x += position.width;
//             position.width = 60;
//
//             // ReSharper disable once InvertIf
//             if (GUI.Button(position, new GUIContent("Register"))) {
//                 WorldGraphEditorWindow window = WGEditorGUI.GetEditorWindowByName("WorldGraph") as WorldGraphEditorWindow;
//                 if (window == null) {
//                     EditorUtility.DisplayDialog("Error", "Open the WorldGraph EditorWindow to register the property", "OK");
//                     return;
//                 }
//
//                 if (attribute is ExposedParameterAttribute attr)
//                     switch (attr.Type) {
//                         case ParameterType.String:
//                             var param = window.worldGraph.stringParameters.Find(x => x.Reference == attr.Name);
//                             if (param != null) {
//                                 EditorUtility.DisplayDialog("Error", "Property already registered", "OK");
//                                 return;
//                             }
//
//                             window.worldGraph.CreateParameter(ParameterType.String, attr.Name);
//                             break;
//                         case ParameterType.Float:
//                             var param2 = window.worldGraph.floatParameters.Find(x => x.Reference == attr.Name);
//                             if (param2 != null) {
//                                 EditorUtility.DisplayDialog("Error", "Property already registered", "OK");
//                                 return;
//                             }
//
//                             window.worldGraph.CreateParameter(ParameterType.Float, attr.Name);
//                             break;
//                         case ParameterType.Int:
//                             var param3 = window.worldGraph.intParameters.Find(x => x.Reference == attr.Name);
//                             if (param3 != null) {
//                                 EditorUtility.DisplayDialog("Error", "Property already registered", "OK");
//                                 return;
//                             }
//
//                             window.worldGraph.CreateParameter(ParameterType.Int, attr.Name);
//                             break;
//                         case ParameterType.Bool:
//                             var param4 = window.worldGraph.boolParameters.Find(x => x.Reference == attr.Name);
//                             if (param4 != null) {
//                                 EditorUtility.DisplayDialog("Error", "Property already registered", "OK");
//                                 return;
//                             }
//
//                             window.worldGraph.CreateParameter(ParameterType.Bool, attr.Name);
//                             break;
//                         default:
//                             throw new ArgumentOutOfRangeException();
//                     }
//
//                 window.Refresh();
//             }
//         }
//     }
//
// }