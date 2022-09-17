using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace ThunderNut.SceneManagement.Editor {

    public class WGSimpleTreeView : TreeView {
        public Action<ExposedParameter> onDoubleClicked;
        private List<ExposedParameter> data;

        private WGSimpleTreeView(TreeViewState tvs, List<ExposedParameter> data) : base(tvs) {
            this.data = data;
            Reload();
        }

        private WGSimpleTreeView(TreeViewState tvs, MultiColumnHeader header, List<ExposedParameter> exposedParameters) : base(tvs,
            header) { }

        protected override void DoubleClickedItem(int id) {
            onDoubleClicked?.Invoke(data[id]);
            base.DoubleClickedItem(id);
        }

        protected override bool CanMultiSelect(TreeViewItem item) {
            return false;
        }

        protected override TreeViewItem BuildRoot() {
            var root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            var allItems = data.Select((param, index) => new TreeViewItem(index, 0, param.Name)).ToList();

            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        public static WGSimpleTreeView Create(ref TreeViewState tvs, List<ExposedParameter> data) {
            tvs ??= new TreeViewState();
            return new WGSimpleTreeView(tvs, data);
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
            var columns = new MultiColumnHeaderState.Column[] { };

            return new MultiColumnHeaderState(columns);
        }
    }

}