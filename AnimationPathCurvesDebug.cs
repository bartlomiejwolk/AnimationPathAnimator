using UnityEngine;
using System.Collections;
using ATP.AnimationPathTools;

[ExecuteInEditMode]
public class AnimationPathCurvesDebug : MonoBehaviour {

    private AnimationPathAnimator animator;
    private AnimatedObjectPath animatedObjectPath;

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

    [Header("Ease curve")]
    public AnimationCurve tiltingCurve;

    // Use this for initialization
    void Awake() {
    }

    void OnEnable() {
        animator = GetComponent<AnimationPathAnimator>();
        animatedObjectPath = GetComponent<AnimatedObjectPath>();

        rotationCurveX = animator.RotationCurves[0];
        rotationCurveY = animator.RotationCurves[1];
        rotationCurveZ = animator.RotationCurves[2];

        pathCurveX = animatedObjectPath.AnimationCurves[0];
        pathCurveY = animatedObjectPath.AnimationCurves[1];
        pathCurveZ = animatedObjectPath.AnimationCurves[2];

        easeCurve = animator.EaseCurve;
        tiltingCurve = animator.TiltingCurve;
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
