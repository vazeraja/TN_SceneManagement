﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.Searcher;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphEditorView : VisualElement, IDisposable {
        public readonly EditorWindow editorWindow;
        public WorldGraphGraphView graphView { get; private set; }
        private readonly WorldGraph graph;

        private string _AssetName;

        private Blackboard exposedParametersBlackboard;
        private Blackboard inspectorBlackboard;
        private MasterPreviewView masterPreviewView;
        private GenericMenu exposedPropertiesItemMenu;

        public WorldGraphEditorToolbar toolbar { get; }

        const string k_PreviewWindowLayoutKey = "TN.WorldGraph.PreviewWindowLayout";
        private WindowDockingLayout previewDockingLayout => m_PreviewDockingLayout;
        private readonly WindowDockingLayout m_PreviewDockingLayout = new WindowDockingLayout {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
        };

        private const string k_InspectorWindowLayoutKey = "TN.WorldGraph.InspectorWindowLayout";
        private WindowDockingLayout inspectorDockingLayout => m_InspectorDockingLayout;
        private readonly WindowDockingLayout m_InspectorDockingLayout = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        private const string k_BlackboardWindowLayoutKey = "TN.WorldGraph.ExposedPropertiesWindowLayout";
        private WindowDockingLayout blackboardDockingLayout => m_BlackboardDockingLayout;
        private readonly WindowDockingLayout m_BlackboardDockingLayout = new WindowDockingLayout {
            dockingTop = true,
            dockingLeft = true,
            verticalOffset = 8,
            horizontalOffset = 8,
            size = new Vector2(20, 30)
        };

        public string assetName {
            get => _AssetName;
            set {
                _AssetName = value;
                inspectorBlackboard.title = _AssetName + " Inspector";
            }
        }

        private readonly BlackboardFieldManipulator blackboardFieldManipulator;

        public WorldGraphEditorView(EditorWindow editorWindow, WorldGraph graph, string graphName) {
            this.editorWindow = editorWindow;
            this.graph = graph;
            _AssetName = graphName;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/WGEditorView"));

            toolbar = new WorldGraphEditorToolbar();
            var toolbarGUI = new IMGUIContainer(() => { toolbar.OnGUI(); });
            Add(toolbarGUI);

            var content = new VisualElement {name = "content"};
            {
                graphView = new WorldGraphGraphView(editorWindow, graph) {name = "GraphView", viewDataKey = "MaterialGraphView"};
                content.Add(graphView);

                DeserializeWindowLayout(ref m_PreviewDockingLayout, k_PreviewWindowLayoutKey);
                DeserializeWindowLayout(ref m_InspectorDockingLayout, k_InspectorWindowLayoutKey);
                DeserializeWindowLayout(ref m_BlackboardDockingLayout, k_BlackboardWindowLayoutKey);

                CreateMasterPreview();
                CreateInspectorBlackboard();
                CreateExposedParametersBlackboard();

                UpdateSubWindowsVisibility();

                toolbar.changeCheck = UpdateSubWindowsVisibility;
                toolbar.showGraphSettings = graphView.ShowGraphSettings;

                graphView.graphViewChanged = OnGraphViewChanged;
                graphView.inspectorBlackboard = inspectorBlackboard;
                graphView.exposedParametersBlackboard = exposedParametersBlackboard;

                RegisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);
            }
            Add(content);

            blackboardFieldManipulator = new BlackboardFieldManipulator(this);

            graphView.Initialize();
            graphView.RegisterPortCallbacks();
        }
        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange) {
            graphViewChange.elementsToRemove?.ForEach(elem => {
                graphView.ClearInspector();
                switch (elem) {
                    case WorldGraphNodeView nodeView:
                        graph.RemoveSubAsset(nodeView.sceneHandle);
                        break;
                    case WorldGraphEdge edge:
                        break;
                    case BlackboardField blackboardField:
                        ExposedParameter exposedParameter = (ExposedParameter) blackboardField.userData;

                        if (graphView.GetNodeByGuid(exposedParameter.GUID) is ExposedParameterNodeView paramNode) {
                            graphView.RemoveParameterGraphNode(paramNode);
                        }
                        
                        graph.RemoveParameter(exposedParameter);
                        break;
                    case ExposedParameterNodeView parameterNodeView:
                        graph.ExposedParameterViewDatas.Remove(parameterNodeView.data);
                        break;
                }
                graphView.UpdateSerializedProperties();
            });

            graphViewChange.edgesToCreate?.ForEach(edgeView => { });
            return graphViewChange;
        }

        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView(graphView, editorWindow, graph) {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, this);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            graphView.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void CreateInspectorBlackboard() {
            inspectorBlackboard = new Blackboard(graphView) {title = "Inspector", subTitle = ""};
            graphView.Add(inspectorBlackboard);
        }

        private void CreateExposedParametersBlackboard() {
            exposedParametersBlackboard = new Blackboard(graphView) {title = "Exposed Parameters", subTitle = "WorldGraph"};
            {
                exposedParametersBlackboard.Add(new BlackboardSection {title = "Exposed Variables"});
                exposedParametersBlackboard.editTextRequested = (_blackboard, element, newValue) => {
                    graphView.UpdateBlackboardFieldName(element, newValue); 
                };

                exposedPropertiesItemMenu = new GenericMenu();

                exposedPropertiesItemMenu.AddItem(new GUIContent("String"), false, () => {
                    var exposedParameter = graph.CreateParameter("String");
                    graphView.CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Float"), false, () => {
                    var exposedParameter = graph.CreateParameter("Float");
                    graphView.CreateBlackboardField(exposedParameter);

                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Int"), false, () => {
                    var exposedParameter = graph.CreateParameter("Int");
                    graphView.CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddItem(new GUIContent("Bool"), false, () => {
                    var exposedParameter = graph.CreateParameter("Bool");
                    graphView.CreateBlackboardField(exposedParameter);
                });
                exposedPropertiesItemMenu.AddSeparator($"/");

                exposedParametersBlackboard.addItemRequested += _ => exposedPropertiesItemMenu.ShowAsContext();
            }

            graphView.Add(exposedParametersBlackboard);
        }


        #region Serialize Window Layouts

        private static void DeserializeWindowLayout(ref WindowDockingLayout layout, string layoutKey) {
            string serializedLayout = EditorUserSettings.GetConfigValue(layoutKey);
            if (!string.IsNullOrEmpty(serializedLayout)) {
                layout = JsonUtility.FromJson<WindowDockingLayout>(serializedLayout) ?? new WindowDockingLayout();
            }
        }

        private void ApplySerializedWindowLayouts(GeometryChangedEvent evt) {
            UnregisterCallback<GeometryChangedEvent>(ApplySerializedWindowLayouts);

            ApplySerializedLayout(inspectorBlackboard, inspectorDockingLayout, k_InspectorWindowLayoutKey);
            ApplySerializedLayout(exposedParametersBlackboard, blackboardDockingLayout, k_BlackboardWindowLayoutKey);
            ApplySerializedLayout(masterPreviewView, previewDockingLayout, k_PreviewWindowLayoutKey);
        }

        private void ApplySerializedLayout(VisualElement target, WindowDockingLayout layout, string layoutKey) {
            layout.ApplySize(target);
            layout.ApplyPosition(target);

            target.RegisterCallback<GeometryChangedEvent>((evt) => {
                layout.CalculateDockingCornerAndOffset(target.layout, graphView.layout);
                layout.ClampToParentWindow();

                string serializedWindowLayout = JsonUtility.ToJson(layout);
                EditorUserSettings.SetConfigValue(layoutKey, serializedWindowLayout);
            });
        }

        private void UpdateSubWindowsVisibility() {
            exposedParametersBlackboard.visible = toolbar.m_UserViewSettings.isBlackboardVisible;
            inspectorBlackboard.visible = toolbar.m_UserViewSettings.isInspectorVisible;
            masterPreviewView.visible = toolbar.m_UserViewSettings.isPreviewVisible;
        }

        #endregion

        public void Dispose() {
            if (graphView != null) {
                toolbar.Dispose();
                graphView.Dispose();

                blackboardFieldManipulator.target.RemoveManipulator(blackboardFieldManipulator);
                graphView = null;
            }
        }
    }

}