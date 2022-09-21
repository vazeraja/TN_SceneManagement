using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class TempWindow : EditorWindow {
        [MenuItem("Tools/Testing")]
        private static void ShowWindow() {
            var window = GetWindow<TempWindow>();
            window.titleContent = new GUIContent("Testing");
            window.Show();
        }

        private WorldGraphEditorToolbar toolbar;
        private MasterPreviewView masterPreviewView;

        const string k_PreviewWindowLayoutKey = "TN.WorldGraph.Testing.PreviewWindowLayout";
        private WindowDockingLayout m_PreviewDockingLayout = new WindowDockingLayout {
            dockingTop = false,
            dockingLeft = false,
            verticalOffset = 8,
            horizontalOffset = 8,
        };

        private void Awake() {
            CreateMasterPreview();
            toolbar = new WorldGraphEditorToolbar {
                changeCheck = UpdateSubWindowsVisibility
            };
        }

        private void OnEnable() {
            DeserializeWindowLayout(ref m_PreviewDockingLayout, k_PreviewWindowLayoutKey);
            UpdateSubWindowsVisibility();
        }

        private void OnDisable() {
            toolbar.changeCheck = null;
            rootVisualElement.Clear();
        }

        private void OnGUI() {
            toolbar.OnGUI();
        }

        private void CreateMasterPreview() {
            masterPreviewView = new MasterPreviewView() {name = "MasterPreview"};

            var masterPreviewViewDraggable = new WindowDraggable(null, rootVisualElement);
            masterPreviewView.AddManipulator(masterPreviewViewDraggable);
            rootVisualElement.Add(masterPreviewView);

            masterPreviewViewDraggable.OnDragFinished += () => {
                ApplySerializedLayout(masterPreviewView, m_PreviewDockingLayout, k_PreviewWindowLayoutKey);
            };
            masterPreviewView.previewResizeBorderFrame.OnResizeFinished += () => {
                ApplySerializedLayout(masterPreviewView, m_PreviewDockingLayout, k_PreviewWindowLayoutKey);
            };
        }

        private void ApplySerializedLayout(VisualElement target, WindowDockingLayout layout, string layoutKey) {
            layout.ApplySize(target);
            layout.ApplyPosition(target);

            target.RegisterCallback<GeometryChangedEvent>((evt) => {
                layout.CalculateDockingCornerAndOffset(target.layout, rootVisualElement.layout);
                layout.ClampToParentWindow();

                string serializedWindowLayout = JsonUtility.ToJson(layout);
                EditorUserSettings.SetConfigValue(layoutKey, serializedWindowLayout);
            });
        }

        private static void DeserializeWindowLayout(ref WindowDockingLayout layout, string layoutKey) {
            string serializedLayout = EditorUserSettings.GetConfigValue(layoutKey);
            if (!string.IsNullOrEmpty(serializedLayout)) {
                layout = JsonUtility.FromJson<WindowDockingLayout>(serializedLayout) ?? new WindowDockingLayout();
            }
        }

        private void UpdateSubWindowsVisibility() {
            masterPreviewView.visible = toolbar.m_UserViewSettings.isPreviewVisible;
        }
    }

}