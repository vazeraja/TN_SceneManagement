using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using PopupWindow = UnityEditor.PopupWindow;

namespace ThunderNut.SceneManagement.Editor {

    public class SceneHandlePopupWindow : PopupWindowContent {
        private SceneHandle m_SceneHandle;
        private SceneHandleEditor m_SceneHandleEditor;

        private bool m_ShouldClose;
        private Vector2 scrollPos;

        public float Width;
        public float Height;

        public SceneHandlePopupWindow(SceneHandle sceneHandle) {
            m_SceneHandle = sceneHandle;
            m_SceneHandleEditor = UnityEditor.Editor.CreateEditor(m_SceneHandle) as SceneHandleEditor;
        }

        public override void OnGUI(Rect rect) {
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }


            using (var horizontalScope = new GUILayout.HorizontalScope(EditorStyles.toolbar)) {
                Texture2D sceneIcon =
                    (Texture2D) EditorGUIUtility.ObjectContent(m_SceneHandle, m_SceneHandle.GetType()).image;
                GUIContent headerContent = new GUIContent($"{m_SceneHandle.name} ({m_SceneHandle.GetType().Name})", sceneIcon);

                GUILayout.Label(headerContent, EditorStyles.boldLabel, GUILayout.ExpandHeight(true));
            }
            
            using (var areaScope = new GUILayout.AreaScope(new Rect(12.5f, 25, (Width - 25), Height - 35)))
            {
                using (var scrollViewScope = new GUILayout.ScrollViewScope(scrollPos)) {
                    scrollPos = scrollViewScope.scrollPosition;

                    m_SceneHandleEditor.drawScriptField = false;
                    m_SceneHandleEditor.OnInspectorGUI();
                }
            }
        }

        public override Vector2 GetWindowSize() {
            var result = base.GetWindowSize();
            result.x = Width;
            result.y = Height;
            return result;
        }

        public override void OnClose() {
            Object.DestroyImmediate(m_SceneHandleEditor);
            m_SceneHandleEditor = null;
            m_SceneHandle = null;

            base.OnClose();
        }

        public void ForceClose() => m_ShouldClose = true;
    }

    [CustomPropertyDrawer(typeof(SceneHandle), true)]
    public class SceneHandlePropertyDrawer : PropertyDrawer {
        private Rect buttonRect;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            position.width -= 60;
            EditorGUI.ObjectField(position, property, label);

            position.x += position.width;
            position.width = 60;

            if (GUI.Button(position, EditorGUIUtility.IconContent("d_SearchWindow"),
                new GUIStyle {alignment = TextAnchor.MiddleCenter}) && property.objectReferenceValue != null) {
                PopupWindow.Show(new Rect(buttonRect.x, buttonRect.y + 10, buttonRect.width, buttonRect.height),
                    new SceneHandlePopupWindow((SceneHandle) property.objectReferenceValue) {
                        Width = buttonRect.width,
                        Height = 400
                    });
            }

            if (Event.current.type == EventType.Repaint)
                buttonRect = GUILayoutUtility.GetLastRect();
        }
    }

}