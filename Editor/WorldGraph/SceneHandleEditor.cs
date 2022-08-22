using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(SceneHandle), true)]
    public class SceneHandleEditor : UnityEditor.Editor {
        public bool drawScriptField = true;

        private SerializedProperty sceneProperty;
        private SerializedProperty passagesProperty;
        private SerializedProperty sceneConnectionsProperty;

        private ReorderableList passagesList;
        private ReorderableList sceneConnectionsList;

        private SceneHandle m_SceneHandle;

        private GUIContent[] availableOptions;

        private void OnEnable() {
            // ------------------------------- Initialize Properties -------------------------------
            m_SceneHandle = target as SceneHandle;

            sceneProperty = serializedObject.FindProperty(nameof(SceneHandle.scene));
            passagesProperty = serializedObject.FindProperty(nameof(SceneHandle.passages));
            sceneConnectionsProperty = serializedObject.FindProperty(nameof(SceneHandle.sceneConnections));

            // ------------------------------- Configure ReorderableLists -------------------------------
            passagesList = new ReorderableList(serializedObject, passagesProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = false,
                

                drawHeaderCallback = rect => { EditorGUI.LabelField(rect, passagesProperty.displayName); },
                drawElementCallback = (rect, index, _, _) => {
                    var element = passagesProperty.GetArrayElementAtIndex(index);
                    var availableIDs = m_SceneHandle.passages;

                    var color = GUI.color;
                    if (string.IsNullOrWhiteSpace(element.stringValue) ||
                        availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1) {
                        GUI.color = new Color(0.69f, 0.41f, 0.18f);
                    }

                    // Draw the property which automatically will select the correct drawer -> a single line text field
                    var elementRect = new Rect(rect.x, rect.y, rect.width, EditorGUI.GetPropertyHeight(element));
                    EditorGUI.PropertyField(elementRect, element);

                    // Reset to the default color
                    GUI.color = color;

                    // If the value is invalid draw a HelpBox
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
                elementHeightCallback = index => {
                    var element = passagesProperty.GetArrayElementAtIndex(index);
                    var availableIDs = m_SceneHandle.passages;

                    float height = EditorGUI.GetPropertyHeight(element);

                    // Increase height for invalid values
                    if (string.IsNullOrWhiteSpace(element.stringValue) ||
                        availableIDs.Count(item => string.Equals(item, element.stringValue)) > 1) {
                        height += EditorGUIUtility.singleLineHeight;
                    }

                    return height;
                },
                onAddCallback = list => {
                    list.serializedProperty.arraySize++;

                    var e = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    e.stringValue = "";
                },
            };

            // Set up PassageElements List as Reorderable list
            sceneConnectionsList = new ReorderableList(serializedObject, sceneConnectionsProperty) {
                displayAdd = true,
                displayRemove = true,
                draggable = true,

                // Display usual name of the PassageElements property
                drawHeaderCallback = rect => EditorGUI.LabelField(rect, sceneConnectionsProperty.displayName),

                // Get the correct display height of elements in the list according to their values
                // -- Add an additional line as a little spacing between elements -- 
                elementHeightCallback = index => {
                    var element = sceneConnectionsProperty.GetArrayElementAtIndex(index);

                    var sceneTag = element.FindPropertyRelative(nameof(SceneConnection.passage));
                    var handle = element.FindPropertyRelative(nameof(SceneConnection.sceneHandle));
                    var handleTags = element.FindPropertyRelative(nameof(SceneConnection.sceneHandlePassage));

                    return EditorGUI.GetPropertyHeight(sceneTag) + EditorGUI.GetPropertyHeight(handle) +
                           EditorGUI.GetPropertyHeight(handleTags) + EditorGUIUtility.singleLineHeight + 10;
                },

                // Overwrite what shall be done when an element is added via the +
                // Reset all values to the defaults for new added elements
                // By default Unity would clone the values from the last or selected element otherwise
                onAddCallback = list => {
                    // This adds the new element but copies all values of the select or last element in the list
                    list.serializedProperty.arraySize++;

                    var newElement =
                        list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                    var sceneTag = newElement.FindPropertyRelative(nameof(SceneConnection.passage));

                    sceneTag.intValue = -1;
                },

                // Draw element callback for how fields should be drawn in a reorderable list
                // All the important stuff happens here
                drawElementCallback = (rect, index, _, _) => {
                    //get the current element's SerializedProperty
                    var element = sceneConnectionsProperty.GetArrayElementAtIndex(index);

                    // Get the nested property fields of the passageElements class
                    var sceneTag = element.FindPropertyRelative(nameof(SceneConnection.passage));
                    var handle = element.FindPropertyRelative(nameof(SceneConnection.sceneHandle));
                    var handleTags = element.FindPropertyRelative(nameof(SceneConnection.sceneHandlePassage));

                    var popUpHeight = EditorGUI.GetPropertyHeight(sceneTag) + 4;

                    // store the original GUI.color
                    var color = GUI.color;

                    // if the value is invalid tint the next field red
                    if (sceneTag.intValue < 0) GUI.color = Color.red;

                    // Draw the Popup so you can select from the existing character names
                    sceneTag.intValue = EditorGUI.Popup(new Rect(rect.x, rect.y, rect.width, popUpHeight),
                        new GUIContent(m_SceneHandle.scene != null
                            ? m_SceneHandle.scene.GetType().Name
                            : sceneTag.displayName),
                        sceneTag.intValue, availableOptions);

                    // reset the GUI.color
                    GUI.color = color;
                    rect.y += popUpHeight;

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
                        ? m_SceneHandle.sceneConnections[index].sceneHandle.passages
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
            };

            // ------------------------------- Initialize Passage Names -------------------------------
            availableOptions = (target as SceneHandle)?.passages.Select(item => new GUIContent(item)).ToArray();
        }

        public override void OnInspectorGUI() {
            if (drawScriptField)
                DrawScriptField();

            serializedObject.Update();

            const bool disabled = true;
            using (new EditorGUI.DisabledGroupScope(disabled)) {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("Active"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("guid"));
            }

            EditorGUILayout.PropertyField(serializedObject.FindProperty("scene"));
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
            passagesList.DoLayoutList();
            if (EditorGUI.EndChangeCheck()) {
                serializedObject.ApplyModifiedProperties();

                availableOptions = m_SceneHandle.passages
                    .Select(item => new GUIContent(item))
                    .ToArray();

                EditorUtility.SetDirty(target);
            }

            sceneConnectionsList.DoLayoutList();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawScriptField() {
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.ObjectField("Script", MonoScript.FromScriptableObject((SceneHandle) target),
                ((SceneHandle) target).GetType(), false);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.Space();
        }

        #region Old/Unused Code

        // [MenuItem("Assets/Create/World Graph/Scene Handle (From Scene)", false, 400)]
        // private static void CreateFromScene() {
        //     var trailingNumbersRegex = new Regex(@"(\d+$)");
        //
        //     var scene = Selection.activeObject as SceneAsset;
        //
        //     var asset = CreateInstance<SceneHandle>();
        //     string baseName = trailingNumbersRegex.Replace(scene != null ? scene.name : string.Empty, "");
        //     asset.name = baseName + "Handle";
        //     if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(scene, out string newGuid, out long _)) {
        //         asset.scene = new SceneReference(newGuid);
        //     }
        //
        //     string assetPath = Path.GetDirectoryName(AssetDatabase.GetAssetPath(scene));
        //     AssetDatabase.CreateAsset(asset, Path.Combine(assetPath ?? Application.dataPath, asset.name + ".asset"));
        //     AssetDatabase.SaveAssets();
        // }
        //
        // [MenuItem("Assets/Create/World Graph/Scene Handle (From Scene)", true, 400)]
        // private static bool CreateFromSceneValidation() => Selection.activeObject as SceneAsset;

        #endregion
    }

}