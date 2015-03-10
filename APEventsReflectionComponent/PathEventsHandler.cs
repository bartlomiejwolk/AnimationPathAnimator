using ATP.AnimationPathAnimator.APAnimatorComponent;
using ATP.AnimationPathAnimator.EventsMessageComponent;
using UnityEngine;

namespace ATP.AnimationPathAnimator.PathEventsHandlerComponent {

    [RequireComponent(typeof(APAnimator))]
    public class PathEventsHandler : MonoBehaviour {

        #region FIELDS
        [SerializeField]
        private GUISkin skin;

        [SerializeField]
        private APAnimator apAnimator;

        [SerializeField]
        private APEventsMessageSettings messageSettings;

        [SerializeField]
        private bool advancedSettingsFoldout;
        #endregion

        #region PROPERTIES
        public APAnimator ApAnimator {
            get { return apAnimator; }
            set { apAnimator = value; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        public APEventsMessageSettings MessageSettings {
            get { return messageSettings; }
            set { messageSettings = value; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnDisable() {
            ApAnimator.NodeReached -= Animator_NodeReached;
        }

        private void OnEnable() {
            if (ApAnimator == null) return; 

            ApAnimator.NodeReached += Animator_NodeReached;
        }

        private void Reset() {
            apAnimator = GetComponent<APAnimator>();
            messageSettings =
                Resources.Load<APEventsMessageSettings>("DefaultPathEventsSettings");
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
            // TODO Move GetGlobalNodePositions() to APAnimator class.
            var nodePositions =
                ApAnimator.PathData.GetGlobalNodePositions(ApAnimator.ThisTransform);

            return nodePositions;
        }

        #endregion   

    }

}