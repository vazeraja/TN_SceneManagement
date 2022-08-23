#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement {

    #if UNITY_EDITOR
    [CustomPropertyDrawer(typeof(SceneReference))]
    public class SceneReferencePropertyDrawer : PropertyDrawer {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            var relative = property.FindPropertyRelative("sceneAsset");
            var path = property.FindPropertyRelative("ScenePath");

            EditorGUI.BeginProperty(position, label, relative);

            using var scope = new EditorGUI.ChangeCheckScope();
            var target = EditorGUI.ObjectField(position, label, relative.objectReferenceValue, typeof(SceneAsset), false);
            if (scope.changed) {
                relative.objectReferenceValue = target;
                path.stringValue = AssetDatabase.GetAssetPath(target);
            }
            
            EditorGUI.EndProperty();
        }
    }
    #endif

    [Serializable]
    public class SceneReference {
        [SerializeField] private Object sceneAsset;
        [SerializeField] public string ScenePath;
    }

    public abstract class SceneHandle : ScriptableObject {
        [Tooltip("Whether or not this handle is active")]
        public bool Active = true;

        [Tooltip("Unique identifier for this handle")]
        public string guid;

        [Tooltip("Name of the handle to be assigned in inspector or through code")]
        public string HandleName = "";
        
        #if UNITY_EDITOR
        public virtual Color HandleColor => Color.white;
        #endif

        public SceneReference scene;
        public List<string> passages = new List<string>() {"default_value1", "default_value2"};
        public List<SceneConnection> sceneConnections;
    }

    [Serializable]
    public class SceneConnection {
        public int passage;
        public SceneHandle sceneHandle;
        public int sceneHandlePassage;
    }

}