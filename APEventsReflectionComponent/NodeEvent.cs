using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsReflectionComponent {

    [System.Serializable]
    // TODO Rename to NodeEventSlot.
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

        [SerializeField]
        private int sourceComponentIndex;

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

        /// <summary>
        /// Selected source component.
        /// </summary>
        public Component SourceCo {
            get { return sourceCo; }
        }

        /// <summary>
        /// Names of components existing on source game object.
        /// </summary>
//[SerializeField]
//private string[] sourceCoNames;
        public string SourceMethodName {
            get { return sourceMethodName; }
        }

        /// <summary>
        /// Selected source game object.
        /// </summary>
        public GameObject SourceGO {
            get { return sourceGO; }
        }

    }

}