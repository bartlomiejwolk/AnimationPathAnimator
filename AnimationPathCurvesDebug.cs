using UnityEngine;
using System.Collections;
using ATP.AnimationPathTools;

[ExecuteInEditMode]
public class AnimationPathCurvesDebug : MonoBehaviour {

    private AnimationPathAnimator animator;
    [Header("Rotation curves")]
    public AnimationCurve curveX;
    public AnimationCurve curveY;
    public AnimationCurve curveZ;

    [Header("Ease curve")]
    public AnimationCurve easeCurve;

    // Use this for initialization
    void Awake() {
    }

    void OnEnable() {
        animator = GetComponent<AnimationPathAnimator>();

        curveX = animator.RotationCurves[0];
        curveY = animator.RotationCurves[1];
        curveZ = animator.RotationCurves[2];
        easeCurve = animator.EaseCurve;
    }

    void Start() {

    }

    // Update is called once per frame
    void Update() {
    }
}
