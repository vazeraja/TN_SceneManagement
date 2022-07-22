using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.LowLevel;

namespace ThunderNut.SceneManagement.Editor {
    
    [CustomEditor(typeof(SceneHandle))]
    public class SceneHandleEditor : UnityEditor.Editor {
        private SerializedProperty sceneTagsProperty;
        private SerializedProperty passageElementsProperty;
        private SerializedProperty sceneProperty;
        
        private ReorderableList sceneTagsList;
        private ReorderableList passageElementsList;

        // Reference to the actual SceneHandle instance this Inspector belongs to
        private SceneHandle sceneHandle;

        // Field for storing available options
        private GUIContent[] availableOptions;

        // Called when the Inspector is opened / ScriptableObject is selected
        private void OnEnable() {
            sceneHandle = (SceneHandle) target;

            // Link in serialized fields to their according SerializedProperties
            sceneTagsProperty = serializedObject.FindProperty(nameof(SceneHandle.sceneTags));
            passageElementsProperty = serializedObject.FindProperty(nameof(SceneHandle.passageElements));

            // Setup and configure the sceneTagsList we will use to display the content of the sceneTagsList 
            sceneTagsList = new ReorderableList(serializedObject, sceneTagsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = false, // for now disable reorder feature since we later go by index!

                // As the header we simply want to see the usual display name 
                drawHeaderCallback = rect => {
                    GUIStyle style = new GUIStyle(EditorStyles.label) {
                        normal = {textColor = Color.black}
                    };

                    EditorGUI.LabelField(rect, sceneTagsProperty.displayName, style);
                },

                // How shall elements be displayed
                drawElementCallback = (rect, index, focused, active) => {
                    // get the current element's SerializedProperty
                    var element = sceneTagsProperty.GetArrayElementAtIndex(index);

                    // Get all characters as string[]
                    var availableIDs = sceneHandle.sceneTags;

                    // store the original GUI.color
                    var color = GUI.color;
                    // Tint the field in red for invalid values
                    // either because it is empty or a duplicate
                    if (string.IsNullOrWhiteSpace(element.stringValue) ||
                        availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1) {
                        GUI.color = new Color(0.69f, 0.41f, 0.18f);
                    }

                    // Draw the property which automatically will select the correct drawer -> a single line text field
                    EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element)),
                        element);

                    // reset to the default color
                    GUI.color = color;

                    // If the value is invalid draw a HelpBox to explain why it is invalid
                    if (string.IsNullOrWhiteSpace(element.stringValue)) {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            "ID may not be empty!", MessageType.Error);
                    }
                    else if (availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1) {
                        rect.y += EditorGUI.GetPropertyHeight(element);
                        EditorGUI.HelpBox(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight),
                            "Duplicate! ID has to be unique!", MessageType.Error);
                    }
                },

                // Get the correct display height of elements in the list according to their values
                elementHeightCallback = index => {
                    var element = sceneTagsProperty.GetArrayElementAtIndex(index);
                    var availableIDs = sceneHandle.sceneTags;

                    var height = EditorGUI.GetPropertyHeight(element);

                    if (string.IsNullOrWhiteSpace(element.stringValue) ||
                        availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1) {
                        height += EditorGUIUtility.singleLineHeight;
                    }

                    return height;
                },

                // Overwrite what shall be done when an element is added via the +
                // Reset all values to the defaults for new added elements
                // By default Unity would clone the values from the last or selected element otherwise
                onAddCallback = list => {
                    // This adds the new element but copies all values of the select or last element in the list
                    list.serializedProperty.arraySize++;

                    var newElement =
                        list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    newElement.stringValue = "";
                }
            };

            // Set up PassageElements List as Reorderable list
            passageElementsList = new ReorderableList(serializedObject, passageElementsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                // Display usual name of the PassageElements property
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, passageElementsProperty.displayName),

                // Get the correct display height of elements in the list according to their values
                // -- Add an additional line as a little spacing between elements -- 
                elementHeightCallback = index => {
                    var element = passageElementsProperty.GetArrayElementAtIndex(index);

                    var sceneTag = element.FindPropertyRelative(nameof(PassageElement.sceneTag));
                    var handle = element.FindPropertyRelative(nameof(PassageElement.sceneHandle));
                    var handleTags = element.FindPropertyRelative(nameof(PassageElement.sceneHandleTags));

                    return EditorGUI.GetPropertyHeight(sceneTag) + EditorGUI.GetPropertyHeight(handle) +
                           EditorGUIUtility.singleLineHeight + 10;
                },

                // Overwrite what shall be done when an element is added via the +
                // Reset all values to the defaults for new added elements
                // By default Unity would clone the values from the last or selected element otherwise
                onAddCallback = list => {
                    // This adds the new element but copies all values of the select or last element in the list
                    list.serializedProperty.arraySize++;

                    var newElement =
                        list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    var sceneTag = newElement.FindPropertyRelative(nameof(PassageElement.sceneTag));
                    var handle = newElement.FindPropertyRelative(nameof(PassageElement.sceneHandle));
                    var text = newElement.FindPropertyRelative(nameof(PassageElement.sceneHandleTags));

                    sceneTag.intValue = -1;
                },
            };

            // Get the existing character names as GuiContent[]
            availableOptions = sceneHandle.sceneTags.Select(item => new GUIContent(item)).ToArray();
        }

        public override void OnInspectorGUI() {
            DrawScriptField();

            serializedObject.Update(); 

            sceneProperty = serializedObject.FindProperty("scene");
            EditorGUILayout.PropertyField(sceneProperty);
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            sceneTagsList.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();

                availableOptions = sceneHandle.sceneTags
                    .Select(item => new GUIContent(item))
                    .ToArray();

                EditorUtility.SetDirty(target);
            }

            passageElementsList.drawElementCallback = DrawPassageElementsFields;
            passageElementsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Draw element callback for how fields should be drawn in a reorderable list
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        private void DrawPassageElementsFields(Rect rect, int index, bool isActive, bool isFocused) {
            //get the current element's SerializedProperty
            var element = passageElementsProperty.GetArrayElementAtIndex(index);

            // Get the nested property fields of the passageElements class
            var sceneTag = element.FindPropertyRelative(nameof(PassageElement.sceneTag));
            var handle = element.FindPropertyRelative(nameof(PassageElement.sceneHandle));
            var handleTags = element.FindPropertyRelative(nameof(PassageElement.sceneHandleTags));

            var popUpHeight = EditorGUI.GetPropertyHeight(sceneTag);

            // store the original GUI.color
            var color = GUI.color;

            // if the value is invalid tint the next field red
            if (sceneTag.intValue < 0) GUI.color = Color.red;

            // Draw the Popup so you can select from the existing character names
            sceneTag.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, popUpHeight),
                new GUIContent(sceneHandle.scene != null ? sceneHandle.scene.GetType().Name : sceneTag.displayName),
                sceneTag.intValue, availableOptions);

            // reset the GUI.color
            GUI.color = color;
            rect.y += popUpHeight + 10;

            var handleHeight = EditorGUI.GetPropertyHeight(handle);
            if (handle.objectReferenceValue == null) GUI.color = Color.red;

            EditorGUI.BeginChangeCheck();
            EditorGUI.PropertyField(new Rect(rect.x, rect.y, rect.width, handleHeight), handle,
                new GUIContent("Target Scene"));
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();
            }

            // Stores tag options based on the chosen scene handle
            var handleOptions = handle.objectReferenceValue != null
                ? sceneHandle.passageElements[index].sceneHandle.sceneTags
                    .Select(item => new GUIContent(item))
                    .ToArray()
                : new GUIContent[] { };

            // reset the GUI.color
            GUI.color = color;
            rect.y += popUpHeight;

            EditorGUI.BeginDisabledGroup(handle.objectReferenceValue == null);
            handleTags.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, popUpHeight),
                new GUIContent("Passage"), handleTags.intValue, handleOptions);
            EditorGUI.EndDisabledGroup();
        }

        #region Other

        private void DrawScriptField() {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((SceneHandle) target),
                typeof(SceneHandle), false);
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
        }

        [MenuItem("Assets/Create/World Graph/Scene Handle (From Scene)", false, 400)]
        private static void CreateFromScene() {
            var trailingNumbersRegex = new Regex(@"(\d+$)");

            var scene = Selection.activeObject as SceneAsset;

            var asset = CreateInstance<SceneHandle>();
            //asset.scene = scene;
            string baseName = trailingNumbersRegex.Replace(scene != null ? scene.name : string.Empty, "");
            asset.name = baseName + "Handle";

            string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(scene));
            AssetDatabase.CreateAsset(asset, Path.Combine(assetPath ?? Application.dataPath, asset.name + ".asset"));
            AssetDatabase.SaveAssets();
        }

        [MenuItem("Assets/Create/World Graph/Scene Handle (From Scene)", true, 400)]
        private static bool CreateFromSceneValidation() => Selection.activeObject as SceneAsset;

        #endregion
    }
}