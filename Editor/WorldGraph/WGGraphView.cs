using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {
    public class WGGraphView : GraphView, IDisposable {
        public new class UxmlFactory : UxmlFactory<WGGraphView, UxmlTraits> { }

        private const string styleSheetPath = "Assets/TN_SceneManagement/Editor/WorldGraph/WorldGraphEditorWindow.uss";

        public WorldGraph graph;

        public BaseEdgeConnectorListener connectorListener;

        public List<WGGraphNode> nodeViews = new List<WGGraphNode>();
        public Dictionary<SceneHandle, WGGraphNode> nodeViewsPerNode = new Dictionary<SceneHandle, WGGraphNode>();
        public List<WGEdge> edgeViews = new List<WGEdge>();
        public List<WGGroup> groupViews = new List<WGGroup>();
        public List<WGStackNode> stackNodeViews = new List<WGStackNode>();
        public Dictionary<Type, WGPinnedElement> pinnedElements = new Dictionary<Type, WGPinnedElement>();
        #if UNITY_2020_1_OR_NEWER
        public List<WGStickyNote> stickyNoteViews = new List<WGStickyNote>();
        #endif

        public WGGraphView() {
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
            styleSheets.Add(styleSheet);

            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        public void Initialize(WorldGraph graph) {
            Debug.Log("WGGraphView: Initialized");
        }

        public void SaveGraphToDisk() {
            if (graph == null)
                return;

            EditorUtility.SetDirty(graph);
        }

        public void Dispose() {
            Debug.Log("Disposed GraphView");

            // ClearGraphElements();
            // RemoveFromHierarchy();
            // Undo.undoRedoPerformed -= ReloadView;
            // Object.DestroyImmediate(nodeInspector);
            // NodeProvider.UnloadGraph(graph);
            // exposedParameterFactory.Dispose();
            // exposedParameterFactory = null;
            // 
            // graph.onExposedParameterListChanged -= OnExposedParameterListChanged;
            // graph.onExposedParameterModified += (s) => onExposedParameterModified?.Invoke(s);
            // graph.onGraphChanges -= GraphChangesCallback;
        }
    }
}