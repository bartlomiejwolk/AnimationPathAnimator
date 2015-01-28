using UnityEngine;
using System.Collections;

namespace ATP.AnimationPathTools {

    /// <summary>
    ///     Keeps references to animation paths and targets used by the
    ///     AnimationPathAnimator class.
    /// </summary>
    [System.Serializable]
	public class Animation {

        /// <summary>
        ///     Transform to be animated.
        /// </summary>
        [SerializeField]
        private Transform target;

        /// <summary>
        ///     Path used to animate the <c>target</c> transform.
        /// </summary>
        [SerializeField]
        private AnimationPath path;

        /// <summary>
        ///     Transform that the <c>target</c> will be looking at.
        /// </summary>
        [SerializeField]
        private Transform lookAtTarget;

        /// <summary>
        ///     Path used to animate the <c>lookAtTarget</c>.
        /// </summary>
        [SerializeField]
        private AnimationPath lookAtPath;

        public Transform Target {
            get { return target; }
        }

        public AnimationPath Path {
            get { return path; }
        }

        public Transform LookAtTarget {
            get { return lookAtTarget; }
        }

        public AnimationPath LookAtPath {
            get { return lookAtPath; }
        }
    }
}
