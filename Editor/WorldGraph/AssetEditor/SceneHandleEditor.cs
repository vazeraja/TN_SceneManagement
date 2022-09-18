using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        private bool _settingsDropdown;
        private Rect buttonRect;

        private SceneHandle sceneHandle;

        private SerializedProperty transitionsProperty;
        private ReorderableList transitionsReorderableList;

        private SerializedProperty childrenProperty;
        private ReorderableList childrenReorderableList;
        private SerializedProperty worldGraphProperty;
        private SerializedProperty activeProperty;
        private SerializedProperty handleNameProperty;
        private SerializedProperty sceneProperty;

        private void OnEnable() {
            sceneHandle = target as SceneHandle;
            
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
            
            worldGraphProperty = serializedObject.FindProperty("WorldGraph");
            activeProperty = serializedObject.FindProperty("Active");
            handleNameProperty = serializedObject.FindProperty("HandleName");
            sceneProperty = serializedObject.FindProperty("scene");
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
            EditorGUILayout.PropertyField(worldGraphProperty);
            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(activeProperty);
            EditorGUILayout.PropertyField(handleNameProperty);

            EditorGUILayout.Space(10);
            EditorGUILayout.PropertyField(sceneProperty);
            childrenReorderableList.DoLayoutList();

            EditorGUILayout.PropertyField(serializedObject.FindProperty("stringParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("floatParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("intParameters"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("boolParameters"));

            serializedObject.ApplyModifiedProperties();
        }
    }

}