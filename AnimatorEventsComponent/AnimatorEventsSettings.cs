using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorEventsComponent {

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