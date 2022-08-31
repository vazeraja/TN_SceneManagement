using UnityEditor;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

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

}