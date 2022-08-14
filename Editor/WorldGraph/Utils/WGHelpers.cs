using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

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

        public static EditorWindow GetEditorWindowByName(string name) {
            return Resources.FindObjectsOfTypeAll<EditorWindow>().ToList()
                .Find(x => x.titleContent.ToString() == name);
        }

        public static IEnumerable<T> FindAssetsByType<T>() where T : UnityEngine.Object {
            string[] guids = AssetDatabase.FindAssets($"t:{typeof(T).ToString().Replace("UnityEngine.", "")}");
            return guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(asset => asset != null).ToList();
        }

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
    }

}