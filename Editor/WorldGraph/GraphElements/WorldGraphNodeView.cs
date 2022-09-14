using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class WorldGraphNodeView : Node, IWorldGraphNodeView {
        public Node gvNode => this;
        public SceneHandle sceneHandle { get; private set; }
        public WorldGraphGraphView graphView => GetFirstAncestorOfType<WorldGraphGraphView>();

        public WorldGraphPort input;
        public WorldGraphPort output;
        public Color portColor;

        private IEdgeConnectorListener connectorListener;

        private Button addParameterButton;
        private Button playSceneButton;
        private TextField titleTextField;

        public WorldGraphNodeView(WorldGraphGraphView graphView, SceneHandle sceneHandle, IEdgeConnectorListener connectorListener) :
            base(AssetDatabase.GetAssetPath(Resources.Load<VisualTreeAsset>("UXML/WGGraphNode"))) {
            this.connectorListener = connectorListener;
            this.sceneHandle = sceneHandle;

            userData = sceneHandle;
            name = sceneHandle.HandleName;
            viewDataKey = sceneHandle.guid;
            style.left = sceneHandle.position.x;
            style.top = sceneHandle.position.y;

            if (string.IsNullOrEmpty(sceneHandle.HandleName)) {
                sceneHandle.HandleName = sceneHandle.GetType().Name;
            }

            addParameterButton = this.Q<Button>("add-parameter-button");
            addParameterButton.style.backgroundImage = Resources.Load<Texture2D>("Sprite-0001");
            playSceneButton = this.Q<Button>("play-button");

            SetupTitleField();

            LoadDefaultPorts(sceneHandle.ports);
            LoadParameterPorts(sceneHandle.ports);

            addParameterButton.clicked += AddParameterPort;
            playSceneButton.clicked += PlayScene;
        }

        private void SetupTitleField() {
            Label titleLabel = this.Q<Label>("title-label");
            {
                titleLabel.Bind(new SerializedObject(sceneHandle));
                titleLabel.bindingPath = "HandleName";

                titleTextField = new TextField {isDelayed = true};
                titleTextField.style.display = DisplayStyle.None;
                titleLabel.parent.Insert(0, titleTextField);

                titleLabel.RegisterCallback<MouseDownEvent>(e => {
                    if (e.clickCount != 2 || e.button != (int) MouseButton.LeftMouse) return;

                    titleTextField.style.display = DisplayStyle.Flex;
                    titleLabel.style.display = DisplayStyle.None;
                    titleTextField.focusable = true;

                    titleTextField.SetValueWithoutNotify(title);
                    titleTextField.Focus();
                    titleTextField.SelectAll();
                });

                titleTextField.RegisterValueChangedCallback(e => CloseAndSaveTitleEditor(e.newValue));

                titleTextField.RegisterCallback<MouseDownEvent>(e => {
                    if (e.clickCount == 2 && e.button == (int) MouseButton.LeftMouse)
                        CloseAndSaveTitleEditor(titleTextField.value);
                });

                titleTextField.RegisterCallback<FocusOutEvent>(e => CloseAndSaveTitleEditor(titleTextField.value));

                void CloseAndSaveTitleEditor(string newTitle) {
                    graphView.RegisterCompleteObjectUndo("Renamed node " + newTitle);
                    sceneHandle.HandleName = newTitle;

                    // hide title TextBox
                    titleTextField.style.display = DisplayStyle.None;
                    titleLabel.style.display = DisplayStyle.Flex;
                    titleTextField.focusable = false;

                    UpdateTitle();
                }

                void UpdateTitle() {
                    title = sceneHandle.HandleName ?? sceneHandle.GetType().Name;
                }
            }
        }


        private void LoadDefaultPorts(IEnumerable<PortData> portData) {
            PortData outputPortData = null;
            PortData inputPortData = null;

            var portDatas = portData.ToList();
            var loadedOutputPort = portDatas.Find(x => x.PortType == PortType.Default && x.PortDirection == "Output");
            var loadedInputPort = portDatas.Find(x => x.PortType == PortType.Default && x.PortDirection == "Input");

            switch (sceneHandle) {
                case BaseHandle _:
                    AddToClassList("base");
                    portColor = Color.white;

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WorldGraphPort(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WorldGraphPort(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    capabilities &= ~Capabilities.Deletable;

                    break;
                case DefaultHandle _:
                    AddToClassList("defaultHandle");
                    portColor = new Color(0.12f, 0.44f, 0.81f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WorldGraphPort(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WorldGraphPort(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case BattleHandle _:
                    AddToClassList("battleHandle");
                    portColor = new Color(0.94f, 0.7f, 0.31f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WorldGraphPort(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WorldGraphPort(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
                case CutsceneHandle _:
                    AddToClassList("cutsceneHandle");
                    portColor = new Color(0.81f, 0.29f, 0.28f);

                    if (loadedOutputPort == null) outputPortData = sceneHandle.CreatePort(viewDataKey, true, true, false, portColor);
                    output = new WorldGraphPort(loadedOutputPort ?? outputPortData, connectorListener, this);
                    outputContainer.Add(output);

                    if (loadedInputPort == null) inputPortData = sceneHandle.CreatePort(viewDataKey, false, true, false, portColor);
                    input = new WorldGraphPort(loadedInputPort ?? inputPortData, connectorListener, this);
                    inputContainer.Add(input);

                    break;
            }
        }

        private void LoadParameterPorts(IEnumerable<PortData> portData) {
            foreach (var data in portData) {
                if (data.PortType == PortType.Parameter) {
                    var parameterPort = new WorldGraphPort(data, connectorListener, this);
                    inputContainer.Add(parameterPort);
                }
            }
        }

        private void AddParameterPort() {
            var portData = sceneHandle.CreatePort(viewDataKey, false, false, true, portColor);
            var parameterPort = new WorldGraphPort(portData, connectorListener, this);

            inputContainer.Add(parameterPort);
            graphView.InitializePortBehavior(parameterPort);
            ScreenCapture.CaptureScreenshot("Assets/TN_SceneManagement/Editor/Resources/temp.png", 1);
        }

        public override void OnSelected() {
            graphView.DrawInspector(sceneHandle);
            base.OnSelected();
        }

        private void PlayScene() {
            EditorSceneManager.OpenScene(sceneHandle.scene.ScenePath);
        }

        public override void SetPosition(Rect newPos) {
            base.SetPosition(newPos);

            sceneHandle.position.x = newPos.xMin;
            sceneHandle.position.y = newPos.yMin;
        }


        public void Dispose() {
            // Debug.Log($"Disposing: {name}");
            addParameterButton = null;
            playSceneButton = null;
        }
    }

}