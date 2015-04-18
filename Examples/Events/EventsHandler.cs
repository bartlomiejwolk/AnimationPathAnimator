using UnityEngine;
using System.Collections;

public class EventsHandler : MonoBehaviour {

    public ParticleSystem ParticleSystem;

    public void Play2ndParticleSystem() {
        ParticleSystem.Play();
    }
}
