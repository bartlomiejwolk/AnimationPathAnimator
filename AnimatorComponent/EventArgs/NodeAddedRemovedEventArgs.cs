/* 
 * Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using System;

namespace AnimationPathAnimator.AnimatorComponent {

    public sealed class NodeAddedRemovedEventArgs : EventArgs {

        public NodeAddedRemovedEventArgs(int nodeIndex, float nodeTimestamp) {
            NodeIndex = nodeIndex;
            NodeTimestamp = nodeTimestamp;
        }

        public int NodeIndex { get; set; }
        public float NodeTimestamp { get; set; }

    }

}