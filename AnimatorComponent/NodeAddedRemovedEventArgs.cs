using System;

namespace ATP.AnimationPathTools.AnimatorComponent {

    public sealed class NodeAddedRemovedEventArgs : EventArgs {

        public NodeAddedRemovedEventArgs(int nodeIndex) {
            NodeIndex = nodeIndex;
        }

        public int NodeIndex { get; set; }

    }

}