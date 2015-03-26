using System;

namespace ATP.AnimationPathTools.AnimatorComponent {

    public sealed class NodeReachedEventArgs : EventArgs {

        public NodeReachedEventArgs(int nodeIndex, float timestamp) {
            NodeIndex = nodeIndex;
            Timestamp = timestamp;
        }

        public int NodeIndex { get; set; }
        public float Timestamp { get; set; }

    }

    // todo move to own file.
    public sealed class NodeAddedRemovedEventArgs : EventArgs {

        public NodeAddedRemovedEventArgs(int nodeIndex) {
            NodeIndex = nodeIndex;
        }

        public int NodeIndex { get; set; }

    }

}