using System;
using System.Collections.Generic;
using UnityEngine;

namespace ATP.SimplePathAnimator {

    public static class Utilities {

        /// <summary>
        /// </summary>
        /// <remarks>
        /// http://forum.unity3d.com/threads/how-to-set-an-animation-curve-to-linear-through-scripting.151683/#post-1121021
        /// </remarks>
        /// <param name="curve"></param>
        public static void SetCurveLinear(AnimationCurve curve) {
            for (var i = 0; i < curve.keys.Length; ++i) {
                float intangent = 0;
                float outtangent = 0;
                var inTangentSet = false;
                var outTangentSet = false;
                Vector2 point1;
                Vector2 point2;
                Vector2 deltapoint;
                var key = curve[i];

                if (i == 0) {
                    intangent = 0; inTangentSet = true;
                }

                if (i == curve.keys.Length - 1) {
                    outtangent = 0; outTangentSet = true;
                }

                if (!inTangentSet) {
                    point1.x = curve.keys[i - 1].time;
                    point1.y = curve.keys[i - 1].value;
                    point2.x = curve.keys[i].time;
                    point2.y = curve.keys[i].value;

                    deltapoint = point2 - point1;
                    intangent = deltapoint.y / deltapoint.x;
                }

                if (!outTangentSet) {
                    point1.x = curve.keys[i].time;
                    point1.y = curve.keys[i].value;
                    point2.x = curve.keys[i + 1].time;
                    point2.y = curve.keys[i + 1].value;

                    deltapoint = point2 - point1;
                    outtangent = deltapoint.y / deltapoint.x;
                }

                key.inTangent = intangent;
                key.outTangent = outtangent;

                curve.MoveKey(i, key);
            }
        }

        public static void RemoveAllCurveKeys(AnimationCurve curve) {
            var keysToRemoveNo = curve.length;
            for (var i = 0; i < keysToRemoveNo; i++) {
                curve.RemoveKey(0);
            }
        }

        public static void HandleUnmodShortcut(
            //KeyCode key,
            KeyCode key,
            Action callback) {
            //bool modKeyPressed) {
            
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode == key
                //&& !modKeyPressed) {
                && Event.current.modifiers == EventModifiers.None) {

                callback();
            }
        }

        public static void HandleModShortcut(
            Action callback,
            KeyCode key,
            bool modKeyPressed) {

            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode == key
                && modKeyPressed) {

                callback();
            }
        }

        public static void ConvertToGlobalCoordinates(
            ref Vector3[] points,
            Transform transform) {

            // Convert to global.
            for (int i = 0; i < points.Length; i++) {
                points[i] = transform.TransformPoint(points[i]);
            }
        }

        public static void ConvertToGlobalCoordinates(
            ref List<Vector3> points,
            Transform transform) {

            for (int i = 0; i < points.Count; i++) {
                points[i] = transform.TransformPoint(points[i]);
            }
        } 
    }
}
