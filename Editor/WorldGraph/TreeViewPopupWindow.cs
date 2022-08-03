using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {
    public class TreeViewPopupWindow : PopupWindowContent
    {
        readonly SearchField m_SearchField;
        readonly TreeView m_TreeView;
        bool m_ShouldClose;

        public float Width { get; set; }

        public TreeViewPopupWindow(TreeView contents, float width)
        {
            m_SearchField = new SearchField();
            m_TreeView = contents;
            Width = width;
        }

        public override void OnGUI(Rect rect)
        {
            // Escape closes the window
            if (m_ShouldClose || Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            {
                GUIUtility.hotControl = 0;
                editorWindow.Close();
                GUIUtility.ExitGUI();
            }

            const int border = 4;
            const int topPadding = 12;
            const int searchHeight = 20;
            const int remainTop = topPadding + searchHeight + border;
            var searchRect = new Rect(border, topPadding, rect.width - border * 2, searchHeight);
            var remainingRect = new Rect(border, topPadding + searchHeight + border, rect.width - border * 2, rect.height - remainTop - border);

            m_TreeView.searchString = m_SearchField.OnGUI(searchRect, m_TreeView.searchString);
            m_TreeView.OnGUI(remainingRect);

            if (m_TreeView.HasSelection())
                ForceClose();
        }

        public override Vector2 GetWindowSize()
        {
            var result = base.GetWindowSize();
            result.x = Width;
            return result;
        }

        public override void OnOpen()
        {
            m_SearchField.SetFocus();
            base.OnOpen();
        }

        public void ForceClose() => m_ShouldClose = true;
    }
}