using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public sealed class WGNodeView : Node, IWorldGraphNodeView {
        public WGGraphView graphView { get; private set; }
        public Node gvNode => this;

        public SceneHandle sceneHandle { get; private set; }

        public Port input;
        public Port output;

        public WGNodeView(WGGraphView graphView, SceneHandle sceneHandle) : base(
            AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) {
            // UseDefaultStyling();

            this.graphView = graphView;
            this.sceneHandle = sceneHandle;

            name = sceneHandle.GetType().Name;
            viewDataKey = sceneHandle.guid;
            style.left = sceneHandle.position.x;
            style.top = sceneHandle.position.y;

            var serializedHandle = new SerializedObject(sceneHandle);
            sceneHandle.HandleName = sceneHandle.GetType().Name;

            switch (sceneHandle) {
                case BaseHandle _:
                    AddToClassList("base");

                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(BaseHandle));

                    AddPortToContainer();
                    break;
                case DefaultHandle _:
                    AddToClassList("defaultHandle");

                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(DefaultHandle));

                    AddPortToContainer();
                    break;
                case BattleHandle _:
                    AddToClassList("battleHandle");

                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(BattleHandle));
                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(BattleHandle));

                    AddPortToContainer();
                    break;
                case CutsceneHandle _:
                    AddToClassList("cutsceneHandle");

                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(CutsceneHandle));
                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(CutsceneHandle));

                    AddPortToContainer();
                    break;
            }


            Label description = this.Q<Label>("title-label");
            {
                description.Bind(serializedHandle);
                description.bindingPath = "HandleName";
                var textField = new TextField();
                extensionContainer.Add(textField);
                RefreshExpandedState();
            }
        }

        private void AddPortToContainer() {
            if (input != null) {
                input.portName = "";
                inputContainer.Add(input);
            }

            // ReSharper disable once InvertIf
            if (output != null) {
                output.portName = "";
                outputContainer.Add(output);
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            Undo.RecordObject(sceneHandle, "WorldGraph: SetPosition()");

            sceneHandle.position.x = newPos.xMin;
            sceneHandle.position.y = newPos.yMin;
        }


        public void Dispose() {
            Debug.Log($"Disposing: {name}");
        }
    }

}