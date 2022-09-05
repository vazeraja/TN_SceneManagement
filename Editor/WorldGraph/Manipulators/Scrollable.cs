// Credit: Found in Unity's ShaderGraph package
// Github: https://github.com/Unity-Technologies/ShaderGraph/blob/master/com.unity.shadergraph/Editor/Drawing/Manipulators/Scrollable.cs

using System;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class Scrollable : MouseManipulator {
        Action<float> m_Handler;

        public Scrollable(Action<float> handler) {
            m_Handler = handler;
        }

        protected override void RegisterCallbacksOnTarget() {
            target.RegisterCallback<WheelEvent>(HandleMouseWheelEvent);
        }

        protected override void UnregisterCallbacksFromTarget() {
            target.UnregisterCallback<WheelEvent>(HandleMouseWheelEvent);
        }

        void HandleMouseWheelEvent(WheelEvent evt) {
            m_Handler(evt.delta.y);
            evt.StopPropagation();
        }
    }

}