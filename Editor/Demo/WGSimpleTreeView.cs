using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class WGSimpleTreeView : TreeView {
        private Action closeWindowCallback;

        public WGSimpleTreeView() : base(new TreeViewState()) {
            Reload();
        }

        private WGSimpleTreeView(TreeViewState tvs, MultiColumnHeader mch, Action closeWindowCallback = null) : base(tvs, mch) {
            Reload();
            this.closeWindowCallback = closeWindowCallback;
        }

        protected override void DoubleClickedItem(int id) {
            closeWindowCallback?.Invoke();
            base.DoubleClickedItem(id);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return true;
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = new List<TreeViewItem> {
                new TreeViewItem {id = 1, depth = 0, displayName = "Animals"},
                new TreeViewItem {id = 2, depth = 1, displayName = "Mammals"},
                new TreeViewItem {id = 3, depth = 2, displayName = "Tiger"},
                new TreeViewItem {id = 4, depth = 2, displayName = "Elephant"},
                new TreeViewItem {id = 5, depth = 2, displayName = "Okapi"},
                new TreeViewItem {id = 6, depth = 2, displayName = "Armadillo"},
                new TreeViewItem {id = 7, depth = 1, displayName = "Reptiles"},
                new TreeViewItem {id = 8, depth = 2, displayName = "Crocodile"},
                new TreeViewItem {id = 9, depth = 2, displayName = "Lizard"},
            };

            SetupParentsAndChildrenFromDepths(root, allItems);

            return root;
        }
        

        public static WGSimpleTreeView Create(ref TreeViewState tvs, ref MultiColumnHeaderState mchs, Action cwc = null) {
            tvs ??= new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(mchs, newHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(mchs, newHeaderState);
            mchs = newHeaderState;

            var header = new MultiColumnHeader(mchs);
            return new WGSimpleTreeView(tvs, header, cwc);
        }

        private static MultiColumnHeaderState CreateHeaderState() {
            var columns = new[] {
                new MultiColumnHeaderState.Column(),
            };

            columns[0].headerContent = new GUIContent("Assets", "Performance counters rendered in a chart");
            columns[0].minWidth = 100;
            columns[0].width = 250;
            columns[0].maxWidth = 5000;
            columns[0].headerTextAlignment = TextAlignment.Left;
            columns[0].canSort = false;
            columns[0].autoResize = false;

            return new MultiColumnHeaderState(columns);
        }
    }

}