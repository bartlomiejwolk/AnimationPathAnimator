using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
    private List<NodeEvent> events; 

    private void Start() {

    }

    private void Update() {

    }
}
