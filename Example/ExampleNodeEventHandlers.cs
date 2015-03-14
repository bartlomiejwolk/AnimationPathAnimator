using UnityEngine;

namespace ATP.AnimationPathAnimator.APEventsMessageComponent {

    public class ExampleNodeEventHandlers : MonoBehaviour {

        public void StartFog(string arg) {
            Debug.Log("Fog started: " + arg);
        }

        public void StartMusic() {
            Debug.Log("Music started.");
        }

    }

}
