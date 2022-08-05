using System.Collections.Generic;
using System.Numerics;

namespace ThunderNut.SceneManagement {
    /// <summary>
    /// Data container for the StackNode views
    /// </summary>
    [System.Serializable]
    public class BaseStackNode {
        public Vector2 position;
        public string title = "New Stack";

        /// <summary>
        /// Is the stack accept drag and dropped nodes
        /// </summary>
        public bool acceptDrop;

        /// <summary>
        /// Is the stack accepting node created by pressing space over the stack node
        /// </summary>
        public bool acceptNode;

        /// <summary>
        /// List of node GUID that are in the stack
        /// </summary>
        /// <typeparam name="string"></typeparam>
        /// <returns></returns>
        public List<string> nodeGUIDs = new List<string>();

        public BaseStackNode(Vector2 position, string title = "Stack", bool acceptDrop = true, bool acceptNode = true) {
            this.position = position;
            this.title = title;
            this.acceptDrop = acceptDrop;
            this.acceptNode = acceptNode;
        }
    }
}