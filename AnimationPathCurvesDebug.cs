using UnityEngine;
using System.Collections;
using ATP.AnimationPathTools;

[ExecuteInEditMode]
public class AnimationPathCurvesDebug : MonoBehaviour {

    private AnimationPathAnimator animator;
    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveZ;

    // Use this for initialization
    void Awake() {
    }

    void OnEnable() {
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
        animator = GetComponent<AnimationPathAnimator>();

        curveX = animator.RotationCurves[0];
        curveY = animator.RotationCurves[1];
        curveZ = animator.RotationCurves[2];
    }
}
