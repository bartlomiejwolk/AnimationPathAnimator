using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsMessageComponent {

    [System.Serializable]
    public sealed class NodeEvent {

        [SerializeField]
        private string methodName;

        [SerializeField]
        private string methodArg;

        public string MethodName {
            get { return methodName; }
            set { methodName = value; }
        }

        public string MethodArg {
            get { return methodArg; }
            set { methodArg = value; }
        }

    }

}