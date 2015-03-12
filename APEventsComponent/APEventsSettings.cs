using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsComponent {

    public sealed class APEventsSettings : ScriptableObject {

        #region FIELDS

        [SerializeField]
        private int defaultNodeLabelHaight = 30;

        [SerializeField]
        private int defaultNodeLabelWidth = 100;

        [SerializeField]
        private int methodNameLabelOffsetX = 30;

        [SerializeField]
        private int methodNameLabelOffsetY = -20;
        
        [SerializeField]
        private bool drawMethodNames = true;
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

        public bool DrawMethodNames {
            get { return drawMethodNames; }
            set { drawMethodNames = value; }
        }

        #endregion
    }

}