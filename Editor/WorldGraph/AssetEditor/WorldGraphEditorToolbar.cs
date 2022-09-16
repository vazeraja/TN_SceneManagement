using System;
using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    
    [Serializable]
    public class UserViewSettings {
        public bool isBlackboardVisible = true;
        public bool isInspectorVisible = true;
        public bool isPreviewVisible = true;
    }

    [Serializable]
    public class WorldGraphEditorToolbar : IDisposable {
        
        const string k_UserViewSettings = "TN.WorldGraph.ToggleSettings";
        public UserViewSettings m_UserViewSettings;

        public Action saveRequested { get; set; }
        public Action saveAsRequested { get; set; }
        public Action showInProjectRequested { get; set; }
        public Action refreshRequested { get; set; }
        public Action showGraphSettings { get; set; }
        public Func<bool> isCheckedOut { get; set; }
        public Action checkOut { get; set; }
        
        public Action changeCheck { get; set; }

        public WorldGraphEditorToolbar() {
            string serializedSettings = EditorUserSettings.GetConfigValue(k_UserViewSettings);
            m_UserViewSettings = JsonUtility.FromJson<UserViewSettings>(serializedSettings) ?? new UserViewSettings();
        }

        public void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            {
                if (GUILayout.Button("Save Asset", EditorStyles.toolbarButton)) {
                    saveRequested?.Invoke();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Save As...", EditorStyles.toolbarButton)) {
                    saveAsRequested();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Show In Project", EditorStyles.toolbarButton)) {
                    showInProjectRequested?.Invoke();
                }

                GUILayout.Space(6);
                if (GUILayout.Button("Refresh", EditorStyles.toolbarButton)) {
                    refreshRequested?.Invoke();
                }
                
                GUILayout.Space(6);
                if (GUILayout.Button("Show Graph Settings", EditorStyles.toolbarButton)) {
                    showGraphSettings?.Invoke();
                }

                GUILayout.FlexibleSpace();

                EditorGUI.BeginChangeCheck();
                m_UserViewSettings.isBlackboardVisible =
                    GUILayout.Toggle(m_UserViewSettings.isBlackboardVisible, "Blackboard", EditorStyles.toolbarButton);

                GUILayout.Space(6);

                m_UserViewSettings.isInspectorVisible =
                    GUILayout.Toggle(m_UserViewSettings.isInspectorVisible, "Graph Inspector", EditorStyles.toolbarButton);

                GUILayout.Space(6);

                m_UserViewSettings.isPreviewVisible =
                    GUILayout.Toggle(m_UserViewSettings.isPreviewVisible, "Main Preview", EditorStyles.toolbarButton);

                if (EditorGUI.EndChangeCheck()) {
                    UserViewSettingsChangeCheck();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        private void UserViewSettingsChangeCheck() {
            string serializedUserViewSettings = JsonUtility.ToJson(m_UserViewSettings);
            EditorUserSettings.SetConfigValue(k_UserViewSettings, serializedUserViewSettings);

            changeCheck();
        }

        public void Dispose() {
            saveRequested = null;
            saveAsRequested = null;
            showInProjectRequested = null;
            refreshRequested = null;
            showGraphSettings = null;
            isCheckedOut = null;
            checkOut = null;

            changeCheck = null;
        }
    }

}