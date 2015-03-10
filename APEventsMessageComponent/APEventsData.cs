using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ATP.AnimationPathAnimator.EventsMessageComponent {

    // TODO Rename to APEventsData.
    public sealed class APEventsData : ScriptableObject {

        [SerializeField]
        private List<NodeEvent> nodeEvents;

        public List<NodeEvent> NodeEvents {
            get { return nodeEvents; }
        }

        public void ResetEvents() {
            NodeEvents.Clear();
        }

        private void OnEnable() {
            if (nodeEvents == null) {
                nodeEvents = new List<NodeEvent>();
            }
        }

    }

}
