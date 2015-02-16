using UnityEngine;
using System.Collections;
using ATP.AnimationPathTools;

[ExecuteInEditMode]
public class AnimationPathDebug : MonoBehaviour {

    private AnimationPathAnimator animator;
    private AnimationPathBuilder animationPathBuilder;

    [Header("Animation Path")]
    public AnimationCurve pathCurveX;
    public AnimationCurve pathCurveY;
    public AnimationCurve pathCurveZ;

    [Header("Rotation curves")]
    public AnimationCurve rotationCurveX;
    public AnimationCurve rotationCurveY;
    public AnimationCurve rotationCurveZ;

    [Header("Ease curve")]
    public AnimationCurve easeCurve;

    [Header("Tilting curve")]
    public AnimationCurve tiltingCurve;

    // Use this for initialization
    void Awake() {
    }

    void OnEnable() {
        animator = GetComponent<AnimationPathAnimator>();
        animationPathBuilder = GetComponent<AnimationPathBuilder>();

        rotationCurveX = animator.RotationCurves[0];
        rotationCurveY = animator.RotationCurves[1];
        rotationCurveZ = animator.RotationCurves[2];

        pathCurveX = animationPathBuilder.ObjectPath[0];
        pathCurveY = animationPathBuilder.ObjectPath[1];
        pathCurveZ = animationPathBuilder.ObjectPath[2];

        easeCurve = animator.EaseCurve;
        tiltingCurve = animator.TiltingCurve;
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
