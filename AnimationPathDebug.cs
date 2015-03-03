using UnityEngine;
using System.Collections;
using ATP.SimplePathAnimator;

[ExecuteInEditMode]
public class AnimationPathDebug : MonoBehaviour {

    private AnimationPathAnimator animator;

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
        //animator = GetComponent<AnimationPathAnimator>();
        //animationPathBuilder = GetComponent<AnimationPathBuilder>();

        //rotationCurveX = animator.RotationPath[0];
        //rotationCurveY = animator.RotationPath[1];
        //rotationCurveZ = animator.RotationPath[2];

        //pathCurveX = animationPathBuilder.PathData.AnimatedObjectPath[0];
        //pathCurveY = animationPathBuilder.PathData.AnimatedObjectPath[1];
        //pathCurveZ = animationPathBuilder.PathData.AnimatedObjectPath[2];

        //easeCurve = animator.EaseCurve;
        //tiltingCurve = animator.TiltingCurve;
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
