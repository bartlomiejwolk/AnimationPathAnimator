using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ATP.SimplePathAnimator.PathAnimatorComponent;

namespace ATP.SimplePathAnimator.PathEventsComponent {

    [RequireComponent(typeof(PathAnimator))]
    public sealed class PathEvents : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private PathAnimator pathAnimator;

        [SerializeField]
        private PathEventsData eventsData;

        [SerializeField]
        private PathEventsSettings settings;

        [SerializeField]
        private bool advancedSettingsFoldout;
        #endregion

        #region PROPERTIES
        public PathAnimator PathAnimator {
            get { return pathAnimator; }
            set { pathAnimator = value; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public PathEventsData EventsData {
            get { return eventsData; }
            set { eventsData = value; }
        }

        public PathEventsSettings Settings {
            get { return settings; }
            set { settings = value; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnDisable() {
            PathAnimator.NodeReached -= Animator_NodeReached;
        }

        private void OnEnable() {
            if (PathAnimator == null) return; 

            PathAnimator.NodeReached += Animator_NodeReached;
        }

        private void Reset() {
            pathAnimator = GetComponent<PathAnimator>();
            settings =
                Resources.Load<PathEventsSettings>("DefaultPathEventsSettings");
            skin = Resources.Load("DefaultPathEventsSkin") as GUISkin;
        }
        #endregion
        #region EVENT HANDLERS
        private void Animator_NodeReached(
                    object sender,
                    NodeReachedEventArgs arg) {

            if (EventsData == null) return;

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
