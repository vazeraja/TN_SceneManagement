﻿using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    [CustomEditor(typeof(WorldGraph))]
    public class WorldGraphEditor : UnityEditor.Editor {
        private WorldGraph _worldGraph;
        private SerializedProperty _sceneHandles;

        private Dictionary<SceneHandle, UnityEditor.Editor> _editors;
        private readonly List<string> typeDisplays = new List<string>();
        private bool _settingsMenuDropdown;
        private static bool _debugView = false;
        private GUIStyle _playingStyle;

        private void OnEnable() {
            // ------------------------------- Initialize Properties -------------------------------

            _worldGraph = target as WorldGraph;
            _sceneHandles = serializedObject.FindProperty("sceneHandles");

            // ------------------------------- Create editors --------------------------------------

            _editors = new Dictionary<SceneHandle, UnityEditor.Editor>();
            for (var i = 0; i < _sceneHandles.arraySize; i++) {
                AddEditor(_sceneHandles.GetArrayElementAtIndex(i).objectReferenceValue as SceneHandle);
            }

            // ------------------------------- Get Display Options ---------------------------------

            typeDisplays.Add("Add new SceneHandle...");
            typeDisplays.AddRange(WGAttributeCache.knownNodeTypes.Select(type => type.Name));

            _playingStyle = new GUIStyle {normal = {textColor = Color.yellow}};
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();

            Undo.RecordObject(target, "Modified WorldGraph");

            EditorGUILayout.Space();

            EditorGUILayout.HelpBox($"Select SceneHandles using the **Add new SceneHandle** button", MessageType.None);

            Rect helpBoxRect = GUILayoutUtility.GetLastRect();

            // -------------------------------------------- Settings dropdown --------------------------------------------

            _settingsMenuDropdown = EditorGUILayout.Foldout(_settingsMenuDropdown, "Settings", true, EditorStyles.foldout);
            if (_settingsMenuDropdown) {
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Initialization", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingA"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingB"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingC"));

                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Other", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingD"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("settingE"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("activeSceneHandle"));
                
                EditorGUILayout.Space(10);
                EditorGUILayout.LabelField("Graph", EditorStyles.boldLabel);
                EditorGUILayout.PropertyField(serializedObject.FindProperty("allParameters"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("ExposedParameterViewDatas"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("transitions"));

            }

            // -------------------------------------------- Duration --------------------------------------------

            float durationRectWidth = 70f;
            Rect durationRect = new Rect(helpBoxRect.xMax - durationRectWidth, helpBoxRect.yMax + 6, durationRectWidth, 17f);
            durationRect.xMin = helpBoxRect.xMax - durationRectWidth;
            durationRect.xMax = helpBoxRect.xMax;

            float playingRectWidth = 70f;
            Rect playingRect = new Rect(helpBoxRect.xMax - playingRectWidth - durationRectWidth, helpBoxRect.yMax + 6,
                playingRectWidth, 17f);
            playingRect.xMin = helpBoxRect.xMax - durationRectWidth - playingRectWidth;
            playingRect.xMax = helpBoxRect.xMax;

            // -------------------------------------------- Direction --------------------------------------------

            float directionRectWidth = 16f;
            Rect directionRect = new Rect(helpBoxRect.xMax - directionRectWidth, helpBoxRect.yMax + 5, directionRectWidth, 17f);
            directionRect.xMin = helpBoxRect.xMax - directionRectWidth;
            directionRect.xMax = helpBoxRect.xMax;

            if (Application.isPlaying) {
                GUI.Label(playingRect, "[PLAYING] ", _playingStyle);
            }

            // -------------------------------------------- Draw list --------------------------------------------

            WGEditorGUI.DrawSection("Scene Handles");

            for (int i = 0; i < _sceneHandles.arraySize; i++) {
                WGEditorGUI.DrawSplitter();

                SerializedProperty property = _sceneHandles.GetArrayElementAtIndex(i);

                if (property.objectReferenceValue == null) continue; // Should not happen ...

                SceneHandle handle = property.objectReferenceValue as SceneHandle;
                System.Diagnostics.Debug.Assert(handle != null, nameof(handle) + " != null");
                handle.hideFlags = _debugView ? HideFlags.None : HideFlags.HideInInspector;

                int id = i;
                bool isExpanded = property.isExpanded;
                string label = handle.name;

                WGEditorGUI.DrawSimpleHeader(ref isExpanded, ref handle.Active, ref handle.HandleName, handle.Color, menu => {
                    if (Application.isPlaying)
                        menu.AddItem(new GUIContent("Play"), false, () => Debug.Log("Play"));
                    else
                        menu.AddDisabledItem(new GUIContent("Play"));
                    menu.AddSeparator(null);
                    menu.AddItem(new GUIContent("Remove"), false, () => RemoveSceneHandle(id));
                });

                property.isExpanded = isExpanded;

                // ReSharper disable once ConvertIfStatementToSwitchStatement
                if (isExpanded) {
                    EditorGUI.BeginDisabledGroup(!handle.Active);
                    EditorGUILayout.Space();

                    if (!_editors.ContainsKey(handle)) AddEditor(handle);

                    UnityEditor.Editor editor = _editors[handle];
                    CreateCachedEditor(handle, handle.GetType(), ref editor);

                    // ((SceneHandleEditor) editor).drawScriptField = false;
                    ((SceneHandleEditor) editor).OnInspectorGUI();
                    //editor.OnInspectorGUI();

                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();

                    EditorGUI.BeginDisabledGroup(!Application.isPlaying);
                    EditorGUILayout.BeginHorizontal();
                    {
                        if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
                            //PlayFeedback(id);
                            Debug.Log("Play");
                        }

                        if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
                            //StopFeedback(id);
                            Debug.Log("Stop");
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUI.EndDisabledGroup();

                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }
            }

            if (_sceneHandles.arraySize > 0) {
                WGEditorGUI.DrawSplitter();
            }

            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            {
                int newItem = EditorGUILayout.Popup(0, typeDisplays.ToArray());
                if (newItem >= 1) {
                    Debug.Log(typeDisplays[newItem]);

                    var type = WGAttributeCache.knownNodeTypes.ToList().Find(x => x.Name == typeDisplays[newItem]);
                    AddSceneHandle(type);
                }
            }
            EditorGUILayout.EndHorizontal();

            // -------------------------------------------- Clean up --------------------------------------------
            var wasRemoved = false;
            for (int i = _sceneHandles.arraySize - 1; i >= 0; i--) {
                // ReSharper disable once InvertIf
                if (_sceneHandles.GetArrayElementAtIndex(i).objectReferenceValue == null) {
                    wasRemoved = true;
                    _sceneHandles.DeleteArrayElementAtIndex(i);
                }
            }

            if (wasRemoved) {
                foreach (var handle in _worldGraph.sceneHandles.Where(handle => handle != null)) {
                    handle.hideFlags = HideFlags.None;
                }
            }

            // -------------------------------------------- Apply Changes --------------------------------------------
            serializedObject.ApplyModifiedProperties();

            // -------------------------------------------- Debug Area --------------------------------------------
            WGEditorGUI.DrawSection("All Scenes Debug");
            EditorGUI.BeginDisabledGroup(!Application.isPlaying);
            EditorGUILayout.BeginHorizontal();
            {
                // -------------------------- Initialize --------------------------
                // if (GUILayout.Button("Initialize", EditorStyles.miniButtonLeft)) {
                //     Debug.Log("Initialize");
                // }

                //  -------------------------- Play button --------------------------
                if (GUILayout.Button("Play", EditorStyles.miniButtonMid)) {
                    Debug.Log("Play");
                }

                // -------------------------- Pause button --------------------------
                // if (GUILayout.Button("Pause", EditorStyles.miniButtonMid)) {
                //     Debug.Log("Pause");
                // }

                // -------------------------- Stop button --------------------------
                if (GUILayout.Button("Stop", EditorStyles.miniButtonMid)) {
                    Debug.Log("Stop");
                }

                // -------------------------- Reset button --------------------------
                // if (GUILayout.Button("Reset", EditorStyles.miniButtonMid)) {
                //     Debug.Log("Reset");
                // }

                EditorGUI.EndDisabledGroup();

                // -------------------------- Revert button --------------------------
                // if (GUILayout.Button("Revert", EditorStyles.miniButtonMid)) {
                //     Debug.Log("Revert");
                // }

                // -------------------------- Debug button --------------------------
                EditorGUI.BeginChangeCheck();
                {
                    _debugView = GUILayout.Toggle(_debugView, "Debug View", EditorStyles.miniButtonRight);

                    if (EditorGUI.EndChangeCheck()) {
                        foreach (var f in _worldGraph.sceneHandles)
                            f.hideFlags = _debugView ? HideFlags.HideInInspector : HideFlags.None;
                        UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();
        }

        private SceneHandle AddSceneHandle(System.Type type) {
            SceneHandle newHandle = _worldGraph.CreateSubAsset(type);
            AddEditor(newHandle);
            return newHandle;
        }

        private void RemoveSceneHandle(int id) {
            SerializedProperty property = _sceneHandles.GetArrayElementAtIndex(id);
            SceneHandle handle = property.objectReferenceValue as SceneHandle;

            (target as WorldGraph)?.RemoveSubAsset(handle);

            _editors.Remove(handle!);
        }

        private void AddEditor(SceneHandle handle) {
            if (handle == null) return;
            if (_editors.ContainsKey(handle)) return;

            UnityEditor.Editor editor = null;
            CreateCachedEditor(handle, null, ref editor);

            _editors.Add(handle, editor);
        }
    }

}