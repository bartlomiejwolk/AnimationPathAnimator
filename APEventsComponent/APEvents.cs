using System.Collections.Generic;
using ATP.AnimationPathAnimator.APAnimatorComponent;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsComponent {

    [RequireComponent(typeof(APAnimator))]
    public class APEvents : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private APAnimator animator;

        [SerializeField]
        private APEventsSettings settings;

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private List<NodeEventSlot> nodeEventSlots;

        [SerializeField]
        private bool drawMethodNames = true;
        #endregion

        #region PROPERTIES
        public APAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public APEventsSettings Settings {
            get { return settings; }
        }

        private List<NodeEventSlot> NodeEventSlots {
            get { return nodeEventSlots; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnDisable() {
            Animator.NodeReached -= Animator_NodeReached;
        }

        private void OnEnable() {
            if (Animator == null) return; 

            Animator.NodeReached += Animator_NodeReached;
        }

        private void Reset() {
            animator = GetComponent<APAnimator>();
            settings =
                Resources.Load<APEventsSettings>("DefaultAPEventsSettings");
            skin = Resources.Load("DefaultAPEventsSkin") as GUISkin;
        }
        #endregion
        #region EVENT HANDLERS
        private void Animator_NodeReached(
                    object sender,
                    NodeReachedEventArgs arg) {

            // Return if there's no event slot created for current path node.
            if (arg.NodeIndex > NodeEventSlots.Count - 1) return;
            // Get event slot.
            var nodeEvent = NodeEventSlots[arg.NodeIndex];
            // Return if source GO was not specified in the event slot.
            if (nodeEvent.SourceGO == null) return;
            // Get method metadata.
            var methodInfo = nodeEvent.SourceCo.GetType()
                    .GetMethod(nodeEvent.SourceMethodName);
            // Return if method info couldn't be loaded.
            if (methodInfo == null) return;
            // Get method parameters.
            var methodParams = methodInfo.GetParameters();

            // Method has no parameters.
            if (methodParams.Length == 0) {
                // Invoke method.
                methodInfo.Invoke(nodeEvent.SourceCo, null);
            }
            // Method has one parameter.
            else if (methodParams.Length == 1) {
                // Return if the parameter is not a string.
                if (methodParams[0].ParameterType.Name != "String") return;
                // Create string parameter argument.
                var stringParam = new object[] {nodeEvent.MethodArg};
                // Invoke method with string parameter.
                methodInfo.Invoke(nodeEvent.SourceCo, stringParam);
            }
        }

        #endregion
        #region METHODS

        private Vector3[] GetNodePositions(int nodesNo) {
            var nodePositions =
                Animator.GetGlobalNodePositions(nodesNo);

            return nodePositions;
        }
        private string[] GetMethodNames() {
            var methodNames = new string[NodeEventSlots.Count];

            for (int i = 0; i < NodeEventSlots.Count; i++) {
                methodNames[i] = NodeEventSlots[i].SourceMethodName;
            }

            return methodNames;
        }

        private bool AssetsLoaded() {
            if (Settings != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        #endregion   

    }

}