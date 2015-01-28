using MemoryManagment;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OneDayGame.LoggingTools;

namespace OneDayGame.AnimationPathTools {

    /// <summary>
    ///     Defines single node in space and time.
    /// </summary>
    public class AnimationPathNode : ScriptableObject, IResetable {

        /// <summary>
        ///     Node position.
        /// </summary>
        [SerializeField]
        private Vector3 position;

        /// <summary>
        ///     Node timestamp.
        /// </summary>
        [SerializeField]
        private float timestamp;

        /// <summary>
        ///     Node in tangent.
        /// </summary>
        [SerializeField]
		private Vector3 inTangent;

        /// <summary>
        ///     Speed value.
        /// </summary>
        [SerializeField]
        private float speed;

        public Vector3 Position {
            get { return position; }
            set { position = value; }
        }

        public float Timestamp {
            get { return timestamp; }
            set { timestamp = value; }
        }

		public Vector3 Tangents {
			get { return inTangent; }
			set { inTangent = value; }
		}

        public float Speed {
            get { return speed; }
            set { speed = value; }
        }

        /// <summary>
        /// Reset class fields.
        /// </summary>
        public void Reset() {
            position.x = 0;
            position.y = 0;
            position.z = 0;

            timestamp = 0;

            inTangent.x = 0;
            inTangent.y = 0;
            inTangent.z = 0;

            speed = 0;
        }
    }
}
