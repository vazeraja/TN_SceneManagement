using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using PropertyAttribute = UnityEngine.PropertyAttribute;

namespace ThunderNut.SceneManagement.Editor {

    public static class WGStyling {
        public static readonly GUIStyle SmallTickbox = new GUIStyle("ShurikenToggle");

        private static readonly Color _splitterDark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
        private static readonly Color _splitterLight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        public static Color Splitter => EditorGUIUtility.isProSkin ? _splitterDark : _splitterLight;

        private static readonly Color _headerBackgroundDark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        private static readonly Color _headerBackgroundLight = new Color(1f, 1f, 1f, 0.4f);
        public static Color HeaderBackground => EditorGUIUtility.isProSkin ? _headerBackgroundDark : _headerBackgroundLight;

        private static readonly Color _reorderDark = new Color(1f, 1f, 1f, 0.2f);
        private static readonly Color _reorderLight = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        public static Color Reorder => EditorGUIUtility.isProSkin ? _reorderDark : _reorderLight;

        private static readonly Color _timingDark = new Color(1f, 1f, 1f, 0.5f);
        private static readonly Color _timingLight = new Color(0f, 0f, 0f, 0.5f);

        private static readonly Texture2D _paneOptionsIconDark;
        private static readonly Texture2D _paneOptionsIconLight;
        public static Texture2D PaneOptionsIcon => EditorGUIUtility.isProSkin ? _paneOptionsIconDark : _paneOptionsIconLight;

        static WGStyling() {
            _paneOptionsIconDark = (Texture2D) EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            _paneOptionsIconLight = (Texture2D) EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
        }

        private static GUIStyle _timingStyle = new GUIStyle();

        /// <summary>
        /// Simply draw a splitter line and a title below
        /// </summary>
        public static void DrawSection(string title) {
            EditorGUILayout.Space();

            DrawSplitter();

            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Draw a separator line
        /// </summary>
        public static void DrawSplitter() {
            // Helper to draw a separator line

            var rect = GUILayoutUtility.GetRect(1f, 1f);

            rect.xMin = 0f;
            rect.width += 4f;

            if (Event.current.type != EventType.Repaint)
                return;

            EditorGUI.DrawRect(rect, Splitter);
        }

        /// <summary>
        /// Draw a header similar to the one used for the post-process stack
        /// </summary>
        public static Rect DrawSimpleHeader(ref bool expanded, ref bool activeField, string title, Color feedbackColor,
            Action<GenericMenu> fillGenericMenu) {
            var e = Event.current;

            // Initialize Rects

            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);

            var reorderRect = backgroundRect;
            reorderRect.xMin -= 8f;
            reorderRect.y += 5f;
            reorderRect.width = 9f;
            reorderRect.height = 9f;

            var foldoutRect = backgroundRect;
            foldoutRect.x += 10f;
            foldoutRect.y += 2f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var toggleRect = backgroundRect;
            toggleRect.x += 25f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;
            
            var labelRect = backgroundRect;
            labelRect.xMin += 45f;
            labelRect.xMax -= 20f;

            var menuIcon = PaneOptionsIcon;
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 1f, menuIcon.width, menuIcon.height);

            var colorRect = new Rect(labelRect.xMin, labelRect.yMin, 5f, 17f);
            colorRect.xMin = 0f;
            colorRect.xMax = 5f;
            EditorGUI.DrawRect(colorRect, feedbackColor);

            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // ------------------------ Background ------------------------
            EditorGUI.DrawRect(backgroundRect, HeaderBackground);

            // ------------------------ Foldout ------------------------
            expanded = GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // ------------------------ Title ------------------------
            using (new EditorGUI.DisabledScope(!activeField)) {
                EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);
            }

            // ------------------------ Active checkbox ------------------------
            activeField = GUI.Toggle(toggleRect, activeField, GUIContent.none, SmallTickbox);

            // ------------------------ Dropdown menu icon ------------------------
            GUI.DrawTexture(menuRect, menuIcon);
            for (var i = 0; i < 3; i++) {
                Rect r = reorderRect;
                r.height = 1;
                r.y = reorderRect.y + reorderRect.height * (i / 3.0f);
                EditorGUI.DrawRect(r, Reorder);
            }

            // Handle events
            if (e.type == EventType.MouseDown) {
                if (menuRect.Contains(e.mousePosition)) {
                    var menu = new GenericMenu();
                    fillGenericMenu(menu);
                    menu.DropDown(new Rect(new Vector2(menuRect.x, menuRect.yMax), Vector2.zero));
                    e.Use();
                }
            }

            // ReSharper disable once InvertIf
            if (e.type == EventType.MouseDown && labelRect.Contains(e.mousePosition) && e.button == 0) {
                expanded = !expanded;
                e.Use();
            }

            return backgroundRect;
        }

        #region Other

        public static EditorWindow GetEditorWindowByName(string name) {
            return Resources.FindObjectsOfTypeAll<EditorWindow>().ToList()
                .Find(x => x.titleContent.ToString() == name);
        }

        public static void RepaintInspector(System.Type t) {
            UnityEditor.Editor[] ed = Resources.FindObjectsOfTypeAll<UnityEditor.Editor>();
            foreach (var t1 in ed) {
                if (t1.GetType() != t) continue;
                t1.Repaint();
                return;
            }
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null).ToList();
        }

        #endregion

        #region Serialized Property Extensions

        public static T GetPropertyAttribute<T>(this SerializedProperty prop, bool inherit) where T : PropertyAttribute {
            if (prop == null) {
                return null;
            }

            Type t = prop.serializedObject.targetObject.GetType();

            FieldInfo f = null;
            PropertyInfo p = null;

            foreach (string name in prop.propertyPath.Split('.')) {
                f = t.GetField(name, (BindingFlags) (-1));

                if (f == null) {
                    p = t.GetProperty(name, (BindingFlags) (-1));
                    if (p == null) {
                        return null;
                    }

                    t = p.PropertyType;
                }
                else {
                    t = f.FieldType;
                }
            }

            T[] attributes;

            if (f != null) {
                attributes = f.GetCustomAttributes(typeof(T), inherit) as T[];
            }
            else if (p != null) {
                attributes = p.GetCustomAttributes(typeof(T), inherit) as T[];
            }
            else {
                return null;
            }

            return attributes != null && attributes.Length > 0 ? attributes[0] : null;
        }

        #endregion
    }

}