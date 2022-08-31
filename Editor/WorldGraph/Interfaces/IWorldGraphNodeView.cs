using System;
using UnityEditor.Experimental.GraphView;

namespace ThunderNut.SceneManagement.Editor {

    public interface IWorldGraphNodeView : IDisposable {
        public Node gvNode { get; }
        public SceneHandle sceneHandle { get; }
    }
}