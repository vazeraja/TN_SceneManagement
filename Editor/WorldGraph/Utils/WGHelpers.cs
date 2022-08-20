using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Profiling;
using PropertyAttribute = UnityEngine.PropertyAttribute;

namespace ThunderNut.SceneManagement.Editor {

    public static class WGHelpers {
        public static string[] Ingredients {
            get {
                return new[] {
                    "Car/Honda/Civic",
                    "Car/Honda/CRV",
                    "Car/BMW/328i",
                    "Electronics/Computer/Keyboard",
                    "Electronics/Computer/Mouse",
                    "Electronics/Computer/Monitor",
                    "Electronics/Computer/Headset",
                    "Electronics/Computer/Microphone",
                    "Meat/Chicken/Roasted",
                    "Meat/Chicken/Tenders",
                    "Meat/Chicken/Wings",
                    "Meat/Chicken/Legs",
                    "Meat/Chicken/Thigh",
                    "Meat/Chicken/Frozen",
                    "Meat/Pork/Joint",
                    "Meat/Pork/Slices",
                    "Meat/Pork/Shoulder",
                    "Meat/Pork/Assorted",
                    "Meat/Pork/Mixed",
                    "Meat/Sausages/Sliced",
                    "Meat/Sausages/Hotdogs",
                    "Meat/Sausages/Froze",
                    "Meat/Sausages/Butcher",
                    "Meat/Sausages/Pizza",
                    "Meat/Sausages/Italian",
                    "Meat/Turkey/Sliced",
                    "Meat/Turkey/Full",
                    "Meat/Turkey/Roasted",
                };
            }
        }

        // public static readonly GUIStyle SmallTickbox = new("ShurikenToggle");

        static readonly Color _splitterdark = new Color(0.12f, 0.12f, 0.12f, 1.333f);
        static readonly Color _splitterlight = new Color(0.6f, 0.6f, 0.6f, 1.333f);
        public static Color Splitter => EditorGUIUtility.isProSkin ? _splitterdark : _splitterlight;

        static readonly Color _headerbackgrounddark = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        static readonly Color _headerbackgroundlight = new Color(1f, 1f, 1f, 0.4f);
        public static Color HeaderBackground => EditorGUIUtility.isProSkin ? _headerbackgrounddark : _headerbackgroundlight;

        static readonly Color _reorderdark = new Color(1f, 1f, 1f, 0.2f);
        static readonly Color _reorderlight = new Color(0.1f, 0.1f, 0.1f, 0.2f);
        public static Color Reorder => EditorGUIUtility.isProSkin ? _reorderdark : _reorderlight;

        static readonly Color _timingDark = new Color(1f, 1f, 1f, 0.5f);
        static readonly Color _timingLight = new Color(0f, 0f, 0f, 0.5f);

        static readonly Texture2D _paneoptionsicondark;
        static readonly Texture2D _paneoptionsiconlight;
        public static Texture2D PaneOptionsIcon => EditorGUIUtility.isProSkin ? _paneoptionsicondark : _paneoptionsiconlight;

        static WGHelpers()
        {
            _paneoptionsicondark = (Texture2D)EditorGUIUtility.Load("Builtin Skins/DarkSkin/Images/pane options.png");
            _paneoptionsiconlight = (Texture2D)EditorGUIUtility.Load("Builtin Skins/LightSkin/Images/pane options.png");
        }
        
        public static void HorizontalScope(Action block) {
            EditorGUILayout.BeginHorizontal();
            block();
            EditorGUILayout.EndHorizontal();
        }

        public static void VerticalScope(Action block) {
            EditorGUILayout.BeginVertical();
            block();
            EditorGUILayout.EndVertical();
        }

        /// <summary>
        /// Simply draw a splitter line and a title below
        /// </summary>
        public static void DrawSection(string title) {
            EditorGUILayout.Space();

            DrawSplitter();

            EditorGUILayout.Space();

            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        }

        /// <summary>
        /// Draw a separator line
        /// </summary>
        private static void DrawSplitter() {
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
        public static Rect DrawSimpleHeader(ref bool expanded, ref bool activeField, string title)
        {
            var e = Event.current;

            // Initialize Rects

            var backgroundRect = GUILayoutUtility.GetRect(1f, 17f);
            
            var reorderRect = backgroundRect;
            reorderRect.xMin -= 8f;
            reorderRect.y += 5f;
            reorderRect.width = 9f;
            reorderRect.height = 9f;

            var labelRect = backgroundRect;
            labelRect.xMin += 32f;
            labelRect.xMax -= 20f;

            var foldoutRect = backgroundRect;
            foldoutRect.y += 1f;
            foldoutRect.width = 13f;
            foldoutRect.height = 13f;

            var toggleRect = backgroundRect;
            toggleRect.x += 16f;
            toggleRect.y += 2f;
            toggleRect.width = 13f;
            toggleRect.height = 13f;

            var menuIcon = PaneOptionsIcon;
            var menuRect = new Rect(labelRect.xMax + 4f, labelRect.y + 4f, menuIcon.width, menuIcon.height);
            
            // Background rect should be full-width
            backgroundRect.xMin = 0f;
            backgroundRect.width += 4f;

            // Background
            EditorGUI.DrawRect(backgroundRect, HeaderBackground);

            // Foldout
            expanded = GUI.Toggle(foldoutRect, expanded, GUIContent.none, EditorStyles.foldout);

            // Title
            EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

            // Active checkbox
            GUIStyle SmallTickbox = new("ShurikenToggle");
            activeField = GUI.Toggle(toggleRect, activeField, GUIContent.none, SmallTickbox);
            
            // Handle events
            
            if (e.type == EventType.MouseDown && labelRect.Contains(e.mousePosition) && e.button == 0)
            {
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

            return attributes is {Length: > 0} ? attributes[0] : null;
        }

        #endregion
    }

}