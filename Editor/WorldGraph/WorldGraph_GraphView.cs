using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WorldGraph_GraphView : GraphView, IDisposable {
        public new class UxmlFactory : UxmlFactory<WorldGraph_GraphView, UxmlTraits> { }

        private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

        public WorldGraph graph;

        public WorldGraph_GraphView() {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            styleSheets.Add(styleSheet);

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void Initialize(WorldGraph graph) {
            Debug.Log("Initialized");
        }

        public void SaveGraphToDisk() {
            if (graph == null)
                return;

            EditorUtility.SetDirty(graph);
        }

        public void Dispose() { }
    }
}