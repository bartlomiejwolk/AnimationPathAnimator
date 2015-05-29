// Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using UnityEngine;

namespace AnimationPathAnimator.AnimatorEventsComponent {

    public sealed class AnimatorEventsSettings : ScriptableObject {
        #region FIELDS

        [SerializeField]
        private int defaultNodeLabelHaight = 30;

        [SerializeField]
        private int defaultNodeLabelWidth = 100;

        [SerializeField]
        private int methodNameLabelOffsetX = 30;

        [SerializeField]
        private int methodNameLabelOffsetY = -20;

        #endregion

        #region PROPERTIES

        public int DefaultNodeLabelHeight {
            get { return defaultNodeLabelHaight; }
            set { defaultNodeLabelHaight = value; }
        }

        public int DefaultNodeLabelWidth {
            get { return defaultNodeLabelWidth; }
            set { defaultNodeLabelWidth = value; }
        }

        public int MethodNameLabelOffsetX {
            get { return methodNameLabelOffsetX; }
            set { methodNameLabelOffsetX = value; }
        }

        public int MethodNameLabelOffsetY {
            get { return methodNameLabelOffsetY; }
        }

        #endregion
    }

}