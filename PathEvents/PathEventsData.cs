using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ATP.SimplePathAnimator.Events {

    public class PathEventsData : ScriptableObject {

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
