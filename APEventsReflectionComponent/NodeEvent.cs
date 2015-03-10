using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsReflectionComponent {

    [System.Serializable]
    public sealed class NodeEvent {

        [SerializeField]
        private GameObject sourceGO;

        [SerializeField]
        private string methodArg;

        public string MethodArg {
            get { return methodArg; }
            set { methodArg = value; }
        }

    }

}