using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphNodeView : Node, IWorldGraphNodeView {
        public Node gvNode => this;
        public SceneHandle sceneHandle { get; private set; }

        public WorldGraphPort input;
        public WorldGraphPort output;

        private readonly IEdgeConnectorListener connectorListener;
        private WorldGraphGraphView graphView;

        public WorldGraphNodeView(WorldGraphGraphView graphView, SceneHandle sceneHandle, IEdgeConnectorListener connectorListener) :
            base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) {
            // UseDefaultStyling();

            this.sceneHandle = sceneHandle;
            this.graphView = graphView;
            this.connectorListener = connectorListener;

            userData = sceneHandle;
            name = sceneHandle.GetType().Name;
            viewDataKey = sceneHandle.guid;
            style.left = sceneHandle.position.x;
            style.top = sceneHandle.position.y;

            var serializedHandle = new SerializedObject(sceneHandle);
            sceneHandle.HandleName = sceneHandle.GetType().Name;

            CreatePorts();
            AddPortsToContainer();

            Label description = this.Q<Label>("title-label");
            {
                description.Bind(serializedHandle);
                description.bindingPath = "HandleName";
                // var textField = new TextField();
                // extensionContainer.Add(textField);
                // RefreshExpandedState();
            }
        }

        private void CreatePorts() {
            switch (sceneHandle) {
                case BaseHandle _:
                    AddToClassList("base");

                    output = new WorldGraphPort(Direction.Output, Port.Capacity.Multi, typeof(bool), connectorListener) {
                        portColor = Color.white
                    };

                    capabilities &= ~Capabilities.Movable;
                    capabilities &= ~Capabilities.Deletable;

                    break;
                case DefaultHandle _:
                    AddToClassList("defaultHandle");

                    input = new WorldGraphPort(Direction.Input, Port.Capacity.Multi, typeof(bool), connectorListener) {
                        portColor = new Color(0.12f, 0.44f, 0.81f)
                    };
                    output = new WorldGraphPort(Direction.Output, Port.Capacity.Multi, typeof(bool), connectorListener) {
                        portColor = new Color(0.12f, 0.44f, 0.81f)
                    };

                    break;
                case BattleHandle _:
                    AddToClassList("battleHandle");

                    input = new WorldGraphPort(Direction.Input, Port.Capacity.Multi,typeof(bool), connectorListener){
                        portColor = new Color(0.94f, 0.7f, 0.31f)
                    };
                    output = new WorldGraphPort(Direction.Output, Port.Capacity.Multi,typeof(bool), connectorListener){
                        portColor = new Color(0.94f, 0.7f, 0.31f)
                    };

                    break;
                case CutsceneHandle _:
                    AddToClassList("cutsceneHandle");

                    input = new WorldGraphPort(Direction.Input, Port.Capacity.Multi, typeof(bool), connectorListener){
                        portColor = new Color(0.81f, 0.29f, 0.28f)
                    };
                    output = new WorldGraphPort(Direction.Output, Port.Capacity.Multi, typeof(bool), connectorListener){
                        portColor = new Color(0.81f, 0.29f, 0.28f)
                    };

                    break;
            }
        }

        private void AddPortsToContainer() {
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

            sceneHandle.position.x = newPos.xMin;
            sceneHandle.position.y = newPos.yMin;
        }


        public void Dispose() {
            Debug.Log($"Disposing: {name}");
        }
    }

}