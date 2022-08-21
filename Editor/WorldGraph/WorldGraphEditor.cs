using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(WorldGraph))]
    public class WorldGraphEditor : UnityEditor.Editor {
        private bool _settingsMenuDropdown;

        private SerializedProperty _sceneHandles;
        private ReorderableList list;
        private readonly List<string> typeDisplays = new();

        private void OnEnable() {
            // -------------------------------
            _sceneHandles = serializedObject.FindProperty("sceneHandles");

            typeDisplays.Add("Add new SceneHandle...");
            typeDisplays.AddRange(WGNodeTypeCache.knownNodeTypes.Select(type => type.Name));

            list = new ReorderableList(serializedObject, _sceneHandles) {
                displayAdd = false,
                displayRemove = false,
                draggable = false,

                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, _sceneHandles.displayName); },
                elementHeightCallback = index => EditorGUI.GetPropertyHeight(_sceneHandles.GetArrayElementAtIndex(index)),
                drawElementCallback = (rect, index, active, focused) => {
                    var element = _sceneHandles.GetArrayElementAtIndex(index);
                    var propertyRect = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element));
                    EditorGUI.PropertyField(propertyRect, element);
                },
                onAddCallback = thisList => {
                    SceneHandle newHandle = CreateInstance<SceneHandle>();
                    Undo.RecordObject((WorldGraph) target, "WorldGraph");

                    thisList.serializedProperty.arraySize++;

                    var newElement =
                        thisList.serializedProperty.GetArrayElementAtIndex(thisList.serializedProperty.arraySize - 1);
                    newElement.objectReferenceValue = newHandle;

                    Undo.RegisterCreatedObjectUndo(newHandle, "WorldGraph");
                    AssetDatabase.SaveAssets();
                }
            };


            // -------------------------------
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            
            EditorGUILayout.HelpBox($"Select SceneHandles from the 'Add new SceneHandle...' button", MessageType.None);

            WGHelpers.DrawSection("Settings");
            EditorGUILayout.BeginHorizontal();
            {
                int newItem = EditorGUILayout.Popup(0, typeDisplays.ToArray());
                if (newItem >= 1) {
                    Debug.Log(typeDisplays[newItem]);
                    list.onAddCallback(list);
                }
            }
            EditorGUILayout.EndHorizontal();

            list.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private SceneHandle AddSceneHandle() {
            SceneHandle newHandle = CreateInstance<SceneHandle>();
            Undo.RecordObject(target as WorldGraph, "WorldGraph");

            var newEditor = AddEditor(newHandle);

            list.serializedProperty.arraySize++;
            var newElement = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
            newElement.objectReferenceValue = newHandle;

            Undo.RegisterCreatedObjectUndo(newHandle, "WorldGraph");
            AssetDatabase.SaveAssets();

            return newHandle;
        }

        private UnityEditor.Editor AddEditor(SceneHandle handle) {
            if (handle == null)
                return null;

            UnityEditor.Editor editor = null;
            CreateCachedEditor(handle, null, ref editor);
            return editor;
        }
    }

}