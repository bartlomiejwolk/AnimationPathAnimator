using ATP.SimplePathAnimator.AnimatorComponent;
using ATP.SimplePathAnimator.EventsMessageComponent;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathEventsHandlerComponent {

    [RequireComponent(typeof(PathAnimator))]
    public class PathEventsHandler : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private PathAnimator pathAnimator;

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

        }

        #endregion
        #region METHODS

        public Vector3[] GetNodePositions() {
            // TODO Move GetGlobalNodePositions() to Animator class.
            var nodePositions =
                PathAnimator.PathData.GetGlobalNodePositions(PathAnimator.ThisTransform);

            return nodePositions;
        }

        #endregion   

    }

}