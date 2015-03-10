using UnityEngine;
using System.Collections;

namespace ATP.AnimationPathAnimator.EventsMessageComponent {

    public sealed class APEventsSettings : ScriptableObject {

        #region FIELDS

        [SerializeField]
        private float defaultNodeLabelHaight = 30;

        [SerializeField]
        private float defaultNodeLabelWidth = 100;

        [SerializeField]
        private int methodNameLabelOffsetX = 30;

        [SerializeField]
        private int methodNameLabelOffsetY = -20;
        
        [SerializeField]
        private bool drawMethodNames = true;
        #endregion

        #region PROPERTIES

        public float DefaultNodeLabelHeight {
            get { return defaultNodeLabelHaight; }
            set { defaultNodeLabelHaight = value; }
        }

        public float DefaultNodeLabelWidth {
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
