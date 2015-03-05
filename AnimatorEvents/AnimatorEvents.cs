using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ATP.SimplePathAnimator.Animator;

namespace ATP.SimplePathAnimator.PathEvents {

    [RequireComponent(typeof(PathAnimator))]
    public class AnimatorEvents : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private Animator.PathAnimator pathAnimator;

        // todo Remove.
        //[SerializeField]
        //private List<NodeEvent> nodeEvents;

        [SerializeField]
        private AnimatorEventsData eventsData;

        [SerializeField]
        private bool drawMethodNames = true;
        #endregion

        #region PROPERTIES
        public PathAnimator PathAnimator {
            get { return pathAnimator; }
            set { pathAnimator = value; }
        }

        //public List<NodeEvent> NodeEvents {
        //    get { return nodeEvents; }
        //}

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public AnimatorEventsData EventsData {
            get { return eventsData; }
            set { eventsData = value; }
        }

        #endregion

        #region UNITY MESSAGES
        private void Update() {

        }

        private void Start() {

        }

        private void OnDisable() {
            PathAnimator.NodeReached -= Animator_NodeReached;
        }

        private void OnEnable() {
            if (PathAnimator == null) return; 

            PathAnimator.NodeReached += Animator_NodeReached;
        }

        private void Reset() {
            PathAnimator = GetComponent<PathAnimator>();
            Skin = Resources.Load("AnimatorEventsDefaultSkin") as GUISkin;
        }
        #endregion
        #region EVENT HANDLERS
        private void Animator_NodeReached(
                    object sender,
                    NodeReachedEventArgs arg) {
            // Return if no event was specified for current and later nodes.
            if (arg.NodeIndex > EventsData.NodeEvents.Count - 1) return;

            // Get NodeEvent for current path node.
            var nodeEvent = EventsData.NodeEvents[arg.NodeIndex];

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

            foreach (var nodeEvent in EventsData.NodeEvents) {
                methodNames.Add(nodeEvent.MethodName);
            }

            return methodNames;
        }

        public Vector3[] GetNodePositions() {
            // TODO Move GetGlobalNodePositions() to Animator class.
            var nodePositions =
                PathAnimator.PathData.GetGlobalNodePositions(PathAnimator.ThisTransform);

            return nodePositions;
        }

        #endregion
    }

}
