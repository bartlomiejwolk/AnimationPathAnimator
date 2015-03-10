using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ATP.SimplePathAnimator.PathEventsComponent {

    // TODO Rename to EventsData.
    public sealed class PathEventsData : ScriptableObject {

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
