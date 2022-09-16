using System;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class WGSimpleTreeView : TreeView {
        public Action<ExposedParameter> onDoubleClicked;
        private List<ExposedParameter> data;

        private WGSimpleTreeView(TreeViewState tvs, MultiColumnHeader mch, List<ExposedParameter> data) : base(tvs, mch) {
            this.data = data;
            Reload();
        }

        protected override void DoubleClickedItem(int id) {
            onDoubleClicked?.Invoke(data[id]);
            base.DoubleClickedItem(id);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }
        
        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = new List<TreeViewItem>();
            for (var index = 0; index < data.Count; index++) {
                var param = data[index];
                allItems.Add(new TreeViewItem(index, 0, param.Name));
            }
            
            SetupParentsAndChildrenFromDepths (root, allItems);
            return root;
        }

        public static WGSimpleTreeView Create(ref TreeViewState tvs, ref MultiColumnHeaderState mchs, List<ExposedParameter> data) {
            tvs ??= new TreeViewState();

            var newHeaderState = CreateHeaderState();
            if (MultiColumnHeaderState.CanOverwriteSerializedFields(mchs, newHeaderState))
                MultiColumnHeaderState.OverwriteSerializedFields(mchs, newHeaderState);
            mchs = newHeaderState;

            var header = new MultiColumnHeader(mchs);
            return new WGSimpleTreeView(tvs, header, data);
        }

        private static MultiColumnHeaderState CreateHeaderState() {
            var columns = new[] {
                new MultiColumnHeaderState.Column(),
            };

            columns[0].headerContent = new GUIContent(text: "Assets", tooltip: "Performance counters rendered in a chart");
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