using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WGNodeView : Node, IWorldGraphNodeView {
        public WGGraphView graphView { get; private set; }
        public Node gvNode => this;

        public SceneHandle sceneHandle { get; private set; }

        public Port input;
        public Port output;

        public WGNodeView(WGGraphView graphView, SceneHandle sceneHandle = null) : base(
            AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/ReanimatorGraphNode"))) {
            UseDefaultStyling();
            
            this.graphView = graphView;
            this.sceneHandle = sceneHandle;

            // name = sceneHandle.GetType().Name;
            // viewDataKey = sceneHandle.guid;
            // style.left = sceneHandle.position.x;
            // style.top = sceneHandle.position.y;
            // 
            // var serializedHandle = new SerializedObject(sceneHandle);
            // sceneHandle.handleName = sceneHandle.GetType().Name;

            // CreateInputPorts();
            // CreateOutputPorts();
            //
            // switch (sceneHandle) {
            //     case YeehawHandle _:
            //         AddToClassList("base");
            //         break;
            //     case DefaultHandle _:
            //         AddToClassList("defaultHandle");
            //         break;
            //     case BattleHandle _:
            //         AddToClassList("battleHandle");
            //         break;
            //     case CutsceneHandle _:
            //         AddToClassList("cutscene");
            //         break;
            // }


            // Label description = this.Q<Label>("title-label");
            // {
            //     description.Bind(serializedHandle);
            //     description.bindingPath = "handleName";
            //     var textField = new TextField();
            //     extensionContainer.Add(textField);
            // }
        }

        private void CreateInputPorts() {
            switch (sceneHandle) {
                case DefaultHandle _:
                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                        typeof(DefaultHandle));
                    break;
                case BattleHandle _:
                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                        typeof(BattleHandle));
                    break;
                case CutsceneHandle _:
                    input = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single,
                        typeof(CutsceneHandle));
                    break;
                case YeehawHandle _:
                    break;
            }

            if (input == null) return;
            input.portName = "";
            inputContainer.Add(input);
        }

        private void CreateOutputPorts() {
            switch (sceneHandle) {
                case DefaultHandle _:
                    break;
                case BattleHandle _:
                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi,
                        typeof(BattleHandle));
                    break;
                case CutsceneHandle _:
                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                        typeof(CutsceneHandle));
                    break;
                case YeehawHandle _:
                    output = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single,
                        typeof(YeehawHandle));
                    break;
            }

            if (output == null) return;
            output.portName = "";
            outputContainer.Add(output);
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