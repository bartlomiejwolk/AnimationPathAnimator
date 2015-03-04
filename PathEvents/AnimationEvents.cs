using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ATP.SimplePathAnimator.Animator;

namespace ATP.SimplePathAnimator.PathEvents {

    public class AnimationEvents : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private Animator.Animator animator;

        [SerializeField]
        private List<NodeEvent> nodeEvents;

        [SerializeField]
        private bool drawMethodNames = true;
        #endregion

        #region PROPERTIES
        public Animator.Animator Animator {
            get { return animator; }
        }

        public List<NodeEvent> NodeEvents {
            get { return nodeEvents; }
        }

        public GUISkin Skin {
            get { return skin; }
        }
        #endregion

        #region UNITY MESSAGES
        private void Update() {

        }

        private void Start() {

        }

        private void OnDisable() {
            Animator.NodeReached -= Animator_NodeReached;
        }

        private void OnEnable() {
            if (Animator == null) return;

            Animator.NodeReached += Animator_NodeReached;
        }
        #endregion
        #region EVENT HANDLERS
        private void Animator_NodeReached(
                    object sender,
                    NodeReachedEventArgs arg) {
            // Return if no event was specified for current and later nodes.
            if (arg.NodeIndex > NodeEvents.Count - 1) return;

            // Get NodeEvent for current path node.
            var nodeEvent = NodeEvents[arg.NodeIndex];

            // Call method that will handle this event.
            gameObject.SendMessage(
                nodeEvent.MethodName,
                nodeEvent.MethodArg,
                SendMessageOptions.DontRequireReceiver);
        }

        #endregion
        #region METHODS
        public List<string> GetMethodNames() {
            var methodNames = new List<string>();

            foreach (var nodeEvent in NodeEvents) {
                methodNames.Add(nodeEvent.MethodName);
            }

            return methodNames;
        }

        public Vector3[] GetNodePositions() {
            // TODO Move GetGlobalNodePositions() to Animator class.
            var nodePositions =
                Animator.PathData.GetGlobalNodePositions(Animator.ThisTransform);

            return nodePositions;
        }

        #endregion
    }

}
