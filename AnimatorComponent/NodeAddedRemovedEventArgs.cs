using System;

namespace ATP.AnimationPathTools.AnimatorComponent {

    public sealed class NodeAddedRemovedEventArgs : EventArgs {

        public NodeAddedRemovedEventArgs(int nodeIndex, float nodeTimestamp) {
            NodeIndex = nodeIndex;
            NodeTimestamp = nodeTimestamp;
        }

        public int NodeIndex { get; set; }
        public float NodeTimestamp { get; set; }

    }

}