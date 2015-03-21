using UnityEngine;

namespace ATP.AnimationPathTools.EventsComponent {

    public sealed class APEventsSettings : ScriptableObject {
        #region FIELDS

        [SerializeField]
        private int defaultNodeLabelHaight = 30;

        [SerializeField]
        private int defaultNodeLabelWidth = 100;

        [SerializeField]
        private bool drawMethodNames = true;

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

        public bool DrawMethodNames {
            get { return drawMethodNames; }
            set { drawMethodNames = value; }
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