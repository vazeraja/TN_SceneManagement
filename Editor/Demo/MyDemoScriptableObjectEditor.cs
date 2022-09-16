using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    
    [CustomEditor(typeof(MyDemoScriptableObject))]
    public class MyDemoScriptableObjectEditor : UnityEditor.Editor {
        private VisualElement _RootElement;
        private VisualTreeAsset _VisualTree;
        
        private void OnEnable() {
            _RootElement = new VisualElement();
            _VisualTree = Resources.Load<VisualTreeAsset>($"UXML/InspectorContainer");

            StyleSheet styleSheet = Resources.Load<StyleSheet>("UXML/InspectorContainer");
            _RootElement.styleSheets.Add(styleSheet);
        }

        public override VisualElement CreateInspectorGUI() {
            _RootElement.Clear();
            _VisualTree.CloneTree(_RootElement);
            
            _RootElement.Q<ScrollView>("content-container").Add(new IMGUIContainer(base.OnInspectorGUI));

            return _RootElement;
        }
    }

}