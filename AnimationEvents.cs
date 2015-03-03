using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using ATP.SimplePathAnimator;

// TODO Move to separate file.
[System.Serializable]
public class NodeEvent {

    [SerializeField]
    private string methodName;

    [SerializeField]
    private string methodArg;

}

public class AnimationEvents : MonoBehaviour {

    [SerializeField]
    private AnimationPathAnimator animator;

    [SerializeField]
    private List<NodeEvent> events;

    private void OnEnable() {
        if (animator == null) return;

    }

    private void OnDisable() {
        
    }

    private void Start() {

    }

    private void Update() {

    }
}
