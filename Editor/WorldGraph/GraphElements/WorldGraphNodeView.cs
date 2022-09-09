using System;
using System.Collections.Generic;
using System.Linq;
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
        public Color portColor;

        private readonly IEdgeConnectorListener connectorListener;
        public WorldGraphGraphView graphView;

        private Button addParameterButton;

        public WorldGraphNodeView(WorldGraphGraphView graphView, SceneHandle sceneHandle, IEdgeConnectorListener connectorListener) :
            base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) {
            this.connectorListener = connectorListener;
            this.sceneHandle = sceneHandle;
            this.graphView = graphView;

            userData = sceneHandle;
            name = sceneHandle.GetType().Name;
            viewDataKey = sceneHandle.guid;
            style.left = sceneHandle.position.x;
            style.top = sceneHandle.position.y;

            var serializedHandle = new SerializedObject(sceneHandle);
            sceneHandle.HandleName = sceneHandle.GetType().Name;

            addParameterButton = this.Q<Button>("left-button");
            addParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0001");

            Label description = this.Q<Label>("title-label");
            description.Bind(serializedHandle);
            description.bindingPath = "HandleName";

            CreateDefaultPorts(sceneHandle.ports);
            LoadParameterPorts(sceneHandle.ports);
        }

        private void CreateDefaultPorts(IEnumerable<PortData> portData) {
            PortData outputPortData = null;
            PortData inputPortData = null;

            var portDatas = portData.ToList();
            var loadedOutputPort = portDatas.ToList().Find(x => x.PortType == PortType.Default && x.PortDirection == "Output");
            var loadedInputPort = portDatas.ToList().Find(x => x.PortType == PortType.Default && x.PortDirection == "Input");

            addParameterButton.clicked += AddParameterPort;

            switch (sceneHandle) {
                case BaseHandle _:
                    AddToClassList("base");
                    portColor = Color.white;

                    if (loadedOutputPort == null)
                        outputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: true, false, portColor);
                    output = new WorldGraphPort(this, loadedOutputPort ?? outputPortData, connectorListener);
                    outputContainer.Add(output);

                    capabilities &= ~Capabilities.Deletable;

                    break;
                case DefaultHandle _:
                    AddToClassList("defaultHandle");
                    portColor = new Color(0.12f, 0.44f, 0.81f);

                    if (loadedOutputPort == null)
                        outputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: true, false, portColor);
                    output = new WorldGraphPort(this, loadedOutputPort ?? outputPortData, connectorListener);
                    outputContainer.Add(output);

                    if (loadedInputPort == null)
                        inputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: false, false, portColor);
                    input = new WorldGraphPort(this, loadedInputPort ?? inputPortData, connectorListener);
                    inputContainer.Add(input);

                    break;
                case BattleHandle _:
                    AddToClassList("battleHandle");
                    portColor = new Color(0.94f, 0.7f, 0.31f);

                    if (loadedOutputPort == null)
                        outputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: true, false, portColor);
                    output = new WorldGraphPort(this, loadedOutputPort ?? outputPortData, connectorListener);
                    outputContainer.Add(output);

                    if (loadedInputPort == null)
                        inputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: false, false, portColor);
                    input = new WorldGraphPort(this, loadedInputPort ?? inputPortData, connectorListener);
                    inputContainer.Add(input);

                    break;
                case CutsceneHandle _:
                    AddToClassList("cutsceneHandle");
                    portColor = new Color(0.81f, 0.29f, 0.28f);

                    if (loadedOutputPort == null)
                        outputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: true, false, portColor);
                    output = new WorldGraphPort(this, loadedOutputPort ?? outputPortData, connectorListener);
                    outputContainer.Add(output);

                    if (loadedInputPort == null)
                        inputPortData = sceneHandle.CreatePort(viewDataKey, isOutput: false, false, portColor);
                    input = new WorldGraphPort(this, loadedInputPort ?? inputPortData, connectorListener);
                    inputContainer.Add(input);

                    break;
            }
        }

        private void AddParameterPort() {
            var portData = sceneHandle.CreatePort(viewDataKey, false, true, portColor);
            var parameterPort = new WorldGraphPort(this, portData, connectorListener);

            inputContainer.Add(parameterPort);
        }

        private void LoadParameterPorts(IEnumerable<PortData> portData) {
            foreach (var data in portData) {
                if (data.PortType == PortType.Parameter) {
                    var parameterPort = new WorldGraphPort(this, data, connectorListener);
                    inputContainer.Add(parameterPort);
                }
            }
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            sceneHandle.position.x = newPos.xMin;
            sceneHandle.position.y = newPos.yMin;
        }


        public void Dispose() {
            // Debug.Log($"Disposing: {name}");
            addParameterButton = null;
        }
    }

}