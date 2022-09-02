using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [CustomPropertyDrawer(typeof(SceneConnectionsList))]
    public class SceneConnectionsListDrawer : PropertyDrawer {
        private readonly Dictionary<string, ReorderableList> _listsPerProp = new Dictionary<string, ReorderableList>();

        ReorderableList GetReorderableList(SerializedProperty prop) {
            SerializedProperty listProperty = prop.FindPropertyRelative("list");

            ReorderableList list;
            if (_listsPerProp.TryGetValue(listProperty.propertyPath, out list)) {
                return list;
            }

            list = new ReorderableList(listProperty.serializedObject, listProperty, draggable: false, true, true, true);
            _listsPerProp[listProperty.propertyPath] = list;

            list.drawHeaderCallback += rect => EditorGUI.LabelField(rect, prop.displayName);

            list.elementHeightCallback += idx => {
                SerializedProperty elementProp = listProperty.GetArrayElementAtIndex(idx);

                var exitSceneProp = elementProp.FindPropertyRelative("exitScene");
                var exitScenePassageProp = elementProp.FindPropertyRelative("exitScenePassage");
                var entrySceneProp = elementProp.FindPropertyRelative("entryScene");
                var entryScenePassageProp = elementProp.FindPropertyRelative("entryScenePassage");

                return EditorGUI.GetPropertyHeight(exitSceneProp) +
                       EditorGUI.GetPropertyHeight(exitScenePassageProp) +
                       EditorGUI.GetPropertyHeight(entrySceneProp) +
                       EditorGUI.GetPropertyHeight(entryScenePassageProp) +
                       EditorGUIUtility.singleLineHeight;
            };

            list.drawElementCallback += (rect, index, isActive, isFocused) => {
                SerializedProperty elementProp = list.serializedProperty.GetArrayElementAtIndex(index);

                var exitSceneProp = elementProp.FindPropertyRelative("exitScene");
                var exitScenePassageProp = elementProp.FindPropertyRelative("exitScenePassage");
                var entrySceneProp = elementProp.FindPropertyRelative("entryScene");
                var entryScenePassageProp = elementProp.FindPropertyRelative("entryScenePassage");

                var popUpHeight = EditorGUI.GetPropertyHeight(exitScenePassageProp) + 4;
                var color = GUI.color;

                // ------------------------- Draw Exit Scene Property -------------------------

                if (exitSceneProp.objectReferenceValue == null) GUI.color = Color.red;
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUI.GetPropertyHeight(exitSceneProp)),
                    exitSceneProp, new GUIContent("Exit Scene"));

                GUI.color = color;
                rect.y += popUpHeight;

                // ------------------------- Draw Exit Scene Passages Popup -------------------------

                SceneHandle exitSceneHandle = exitSceneProp.objectReferenceValue as SceneHandle;
                GUIContent[] exitSceneHandlePassageOptions = exitSceneHandle != null
                    ? exitSceneHandle.passages.Select(item => new GUIContent(item)).ToArray()
                    : new GUIContent[] { };

                EditorGUI.BeginDisabledGroup(exitSceneHandle == null);
                exitScenePassageProp.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y + 2, rect.width, popUpHeight),
                    new GUIContent(exitScenePassageProp.displayName),
                    exitScenePassageProp.intValue, exitSceneHandlePassageOptions);
                EditorGUI.EndDisabledGroup();

                GUI.color = color;
                rect.y += popUpHeight;

                // ------------------------- Draw Entry Scene Property -------------------------

                if (entrySceneProp.objectReferenceValue == null) GUI.color = Color.red;

                EditorGUI.BeginChangeCheck();
                EditorGUI.PropertyField(new Rect(rect.x, rect.y + 2, rect.width, EditorGUI.GetPropertyHeight(entrySceneProp)),
                    entrySceneProp, new GUIContent("Entry Scene"));
                if (EditorGUI.EndChangeCheck()) {
                    listProperty.serializedObject.ApplyModifiedProperties();
                }

                GUI.color = color;
                rect.y += popUpHeight;

                // ------------------------- Draw Entry Scene Passages Popup -------------------------

                SceneHandle entrySceneHandle = entrySceneProp.objectReferenceValue as SceneHandle;
                GUIContent[] entrySceneHandlePassageOptions = entrySceneHandle != null
                    ? entrySceneHandle.passages.Select(item => new GUIContent(item)).ToArray()
                    : new GUIContent[] { };

                EditorGUI.BeginDisabledGroup(entrySceneHandle == null);
                entryScenePassageProp.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y + 2, rect.width, popUpHeight),
                    new GUIContent("Passage"), entryScenePassageProp.intValue, entrySceneHandlePassageOptions);
                EditorGUI.EndDisabledGroup();
            };
            return list;
        }

        public override void OnGUI(Rect rect, SerializedProperty serializedProperty, GUIContent label) {
            ReorderableList list = GetReorderableList(serializedProperty);

            list.DoList(rect);
        }

        public override float GetPropertyHeight(SerializedProperty serializedProperty, GUIContent label) {
            return GetReorderableList(serializedProperty).GetHeight();
        }
    }

}