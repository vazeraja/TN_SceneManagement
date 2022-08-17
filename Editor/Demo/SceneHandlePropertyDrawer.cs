using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [CustomPropertyDrawer(typeof(SceneHandle))]
    public class SceneHandlePropertyDrawer : PropertyDrawer {
        private Dictionary<string, ReorderableList> reorderableLists = new();

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty passages = property.FindPropertyRelative("passages");

            if (!reorderableLists.ContainsKey(property.propertyPath) || reorderableLists[property.propertyPath].index >
                reorderableLists[property.propertyPath].count - 1) {
                var newList = new ReorderableList(
                    passages.serializedObject, passages, false, true, true, true
                );
                newList.drawHeaderCallback += rect => {
                    EditorGUI.LabelField(rect, passages.displayName);
                    
                };
                newList.drawElementCallback += (rect, index, active, focused) => {
                    var element = passages.GetArrayElementAtIndex(index);
                    
                    var duplicateCount = 0;
                    for (var i = 0; i < passages.arraySize; i++) {
                        if (element.stringValue == passages.GetArrayElementAtIndex(i).stringValue) {
                            duplicateCount++;
                        }
                    }

                    var color = GUI.color;
                    if (string.IsNullOrWhiteSpace(element.stringValue) || duplicateCount > 1) 
                    {
                        GUI.color = new Color(0.69f, 0.41f, 0.18f);
                    }

                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element)),
                        element);

                    GUI.color = color;

                    if (string.IsNullOrWhiteSpace(element.stringValue)) {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            "ID may not be empty!", MessageType.Error);
                    }
                    else if (duplicateCount > 1) {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            "Duplicate! ID has to be unique!", MessageType.Error);
                    }
                };
                newList.elementHeightCallback += index => {
                    var element = passages.GetArrayElementAtIndex(index);
                    var height = EditorGUI.GetPropertyHeight(element);
                    
                    var duplicateCount = 0;
                    for (var i = 0; i < passages.arraySize; i++) {
                        if (element.stringValue == passages.GetArrayElementAtIndex(i).stringValue) {
                            duplicateCount++;
                        }
                    }

                    if (string.IsNullOrWhiteSpace(element.stringValue) || duplicateCount > 1) {
                        height += EditorGUIUtility.singleLineHeight;
                    }

                    return height;
                };
                newList.onAddCallback += list => {
                    list.serializedProperty.arraySize++;

                    var newElement =
                        list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    newElement.stringValue = "";
                };

                reorderableLists[property.propertyPath] = newList;
            }

            return reorderableLists[property.propertyPath].GetHeight();
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position = EditorGUI.IndentedRect(position);
            reorderableLists[property.propertyPath].DoList(position);
        }
    }

}