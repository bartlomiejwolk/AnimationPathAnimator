/* 
 * Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using System;

namespace AnimationPathTools.AnimatorComponent {

    public sealed class NodeReachedEventArgs : EventArgs {

        public NodeReachedEventArgs(int nodeIndex, float timestamp) {
            NodeIndex = nodeIndex;
            Timestamp = timestamp;
        }

        public int NodeIndex { get; set; }
        public float Timestamp { get; set; }

    }

}