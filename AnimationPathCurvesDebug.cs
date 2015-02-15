using UnityEngine;
using System.Collections;
using ATP.AnimationPathTools;

[ExecuteInEditMode]
public class AnimationPathCurvesDebug : MonoBehaviour {

    private AnimationPathAnimator animator;
    private AnimationPath animationPath;

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

    // Use this for initialization
    void Awake() {
    }

    void OnEnable() {
        animator = GetComponent<AnimationPathAnimator>();
        animationPath = GetComponent<AnimationPath>();

        rotationCurveX = animator.RotationCurves[0];
        rotationCurveY = animator.RotationCurves[1];
        rotationCurveZ = animator.RotationCurves[2];

        pathCurveX = animationPath.AnimationCurves[0];
        pathCurveY = animationPath.AnimationCurves[1];
        pathCurveZ = animationPath.AnimationCurves[2];

        easeCurve = animator.EaseCurve;
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
