using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class ConditionOptionsPopupWindow : PopupWindowContent {
        private readonly SearchField m_SearchField;

        private readonly WGSimpleTreeView multiColumnTreeView;
        private readonly TreeViewState multiColumnTreeViewState;

        private bool m_ShouldClose;
        public float Width;

        public ConditionOptionsPopupWindow(List<ExposedParameter> parameters, SerializedProperty property) {
            m_SearchField = new SearchField();
            multiColumnTreeView = WGSimpleTreeView.Create(ref multiColumnTreeViewState, parameters);
            multiColumnTreeView.onDoubleClicked = parameter => {
                property.objectReferenceValue = parameter;
                property.serializedObject.ApplyModifiedProperties();
                ForceClose();
            };
        }

        public override void OnGUI(Rect rect) {
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape) {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2,
                rect.height - remainTop - border);

            multiColumnTreeView.searchString = m_SearchField.OnGUI(searchRect, multiColumnTreeView.searchString);
            multiColumnTreeView.OnGUI(remainingRect);
        }

        public override Vector2 GetWindowSize() {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen() {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        private void ForceClose() => m_ShouldClose = true;
    }

}