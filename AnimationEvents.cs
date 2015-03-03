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

    public string MethodName {
        get { return methodName; }
    }

    public string MethodArg {
        get { return methodArg; }
    }

}

public class AnimationEvents : MonoBehaviour {

    [SerializeField]
    private AnimationPathAnimator animator;

    [SerializeField]
    private List<NodeEvent> nodeEvents;

    public AnimationPathAnimator Animator {
        get { return animator; }
    }

    public List<NodeEvent> NodeEvents {
        get { return nodeEvents; }
    }

    private void OnEnable() {
        if (Animator == null) return;

        Animator.NodeReached += Animator_NodeReached;
    }

    void Animator_NodeReached(object sender, NodeReachedEventArgs arg) {
        // Return if no event was specified for current and later nodes.
        if (arg.NodeIndex > NodeEvents.Count - 1) return;

        // Get NodeEvent for current path node.
        var nodeEvent = NodeEvents[arg.NodeIndex];

        // Call method that will handle this event.
        gameObject.SendMessage(
            nodeEvent.MethodName,
            nodeEvent.MethodArg,
            SendMessageOptions.DontRequireReceiver);
    }

    private void OnDisable() {
        Animator.NodeReached -= Animator_NodeReached;
    }

    private void Start() {

    }

    private void Update() {

    }
}
