// Credit: Found in Unity's ShaderGraph package 
// Github: https://github.com/Unity-Technologies/ShaderGraph/blob/master/com.unity.shadergraph/Editor/Drawing/Manipulators/ResizeBorderFrame.cs

using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class ResizeBorderFrame : VisualElement {
        private List<ResizeSideHandle> m_ResizeSideHandles;

        private bool m_MaintainAspectRatio;
        public bool maintainAspectRatio {
            get => m_MaintainAspectRatio;
            set {
                m_MaintainAspectRatio = value;
                foreach (ResizeSideHandle resizeHandle in m_ResizeSideHandles) {
                    resizeHandle.maintainAspectRatio = value;
                }
            }
        }

        // ReSharper disable once UnassignedField.Global
        public Action OnResizeFinished;

        public ResizeBorderFrame(VisualElement target) {
            InitializeResizeBorderFrame(target, target);
        }

        public ResizeBorderFrame(VisualElement target, VisualElement container) {
            InitializeResizeBorderFrame(target, container);
        }

        private void InitializeResizeBorderFrame(VisualElement target, VisualElement container) {
            pickingMode = PickingMode.Ignore;

            AddToClassList("resizeBorderFrame");

            m_ResizeSideHandles = new List<ResizeSideHandle> {
                new ResizeSideHandle(target, container, ResizeHandleAnchor.BottomRight)
            };

            foreach (ResizeSideHandle resizeHandle in m_ResizeSideHandles) {
                resizeHandle.OnResizeFinished += HandleResizeFinished;
                Add(resizeHandle);
            }
        }

        private void HandleResizeFinished() {
            OnResizeFinished?.Invoke();
        }
    }

}