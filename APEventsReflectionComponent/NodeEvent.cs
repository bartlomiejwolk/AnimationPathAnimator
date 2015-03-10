using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsReflectionComponent {

    [System.Serializable]
    public sealed class NodeEvent {

        /// <summary>
        /// Selected source game object.
        /// </summary>
        [SerializeField]
        private GameObject sourceGO;

        /// <summary>
        /// Selected source component.
        /// </summary>
        [SerializeField]
        private Component sourceCo;

        //[SerializeField]
        //private Component[] sourceComponents;

        /// <summary>
        /// Names of components existing on source game object.
        /// </summary>
        //[SerializeField]
        //private string[] sourceCoNames;

        [SerializeField]
        private string sourceMethodName;

        [SerializeField]
        private int sourceMethodIndex;

        /// <summary>
        /// Index of the source component in the source component names list.
        /// </summary>
        //[SerializeField]
        //private int sourceCoIndex;

        [SerializeField]
        private string methodArg;

        public string MethodArg {
            get { return methodArg; }
            set { methodArg = value; }
        }

    }

}