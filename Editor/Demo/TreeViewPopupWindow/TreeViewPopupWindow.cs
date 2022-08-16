using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    [Serializable]
    public class TreeViewPopupWindow : PopupWindowContent {
        private readonly SearchField m_SearchField;
        private readonly TreeView m_TreeView;
        private bool m_ShouldClose;

        public float Width { get; set; }
        
        private WGSimpleTreeView multiColumnTreeView;
        [SerializeField] private TreeViewState multiColumnTreeViewState;
        [SerializeField] private MultiColumnHeaderState multiColumnHeaderState;

        public TreeViewPopupWindow() {
            m_SearchField = new SearchField();
            multiColumnTreeView = WGSimpleTreeView.Create(ref multiColumnTreeViewState, ref multiColumnHeaderState, ForceClose);
        }

        public override void OnGUI(Rect rect) {
            // Escape closes the window
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

        public override void OnClose() {
            multiColumnTreeView = null;
            base.OnClose();
        }

        public void ForceClose() => m_ShouldClose = true;
    }

}