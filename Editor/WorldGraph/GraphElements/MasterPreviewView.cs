using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ThunderNut.SceneManagement.Editor {

    public class MasterPreviewView : VisualElement {
        private readonly WorldGraph m_Graph;
        private WorldGraphGraphView graphView;
        private EditorWindow editorWindow;

        private readonly Label m_Title;

        public VisualElement preview { get; set; }
        public Image previewTextureView { get; set; } = new Image();
        private Vector2 m_PreviewScrollPosition;

        private ResizeBorderFrame m_PreviewResizeBorderFrame;
        public ResizeBorderFrame previewResizeBorderFrame => m_PreviewResizeBorderFrame;

        public MasterPreviewView(WorldGraphGraphView mGraphView, EditorWindow mEditorWindow, WorldGraph graph) {
            graphView = mGraphView;
            editorWindow = mEditorWindow;
            m_Graph = graph;

            styleSheets.Add(Resources.Load<StyleSheet>("Styles/PreviewView"));

            var topContainer = new VisualElement() {name = "top"};
            {
                m_Title = new Label {
                    name = "title",
                    text = "Main Preview"
                };

                topContainer.Add(m_Title);
            }
            Add(topContainer);

            preview = new VisualElement {name = "middle"};
            {
                previewTextureView = CreatePreview(Texture2D.redTexture);
                m_PreviewScrollPosition = new Vector2(0f, 0f);
                preview.Add(previewTextureView);
                
                preview.AddManipulator(new Scrollable((x) => { }));
            }
            Add(preview);

            m_PreviewResizeBorderFrame = new ResizeBorderFrame(this, this) {name = "resizeBorderFrame", maintainAspectRatio = true};
            Add(m_PreviewResizeBorderFrame);
        }

        private Image CreatePreview(Texture2D texture) {
            var image = new Image {name = "preview", image = texture, scaleMode = ScaleMode.ScaleAndCrop};
            return image;
        }
    }

}