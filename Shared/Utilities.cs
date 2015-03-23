using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public static class Utilities {

        public static void ConvertToGlobalCoordinates(
            ref Vector3[] points,
            Transform transform) {

            // Convert to global.
            for (var i = 0; i < points.Length; i++) {
                points[i] = transform.TransformPoint(points[i]);
            }
        }

        public static void ConvertToGlobalCoordinates(
            ref List<Vector3> points,
            Transform transform) {

            for (var i = 0; i < points.Count; i++) {
                points[i] = transform.TransformPoint(points[i]);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="epsilon"></param>
        /// <returns></returns>
        /// <remarks>http://stackoverflow.com/questions/3874627/floating-point-comparison-functions-for-c-sharp</remarks>
        public static bool FloatsEqual(float a, float b, float epsilon) {
            var absA = Math.Abs(a);
            var absB = Math.Abs(b);
            var diff = Math.Abs(a - b);

            if (a == b) {
                // shortcut, handles infinities
                return true;
            }
            if (a == 0 || b == 0) {
                // a or b is zero or both are extremely close to it
                // relative error is less meaningful here
                return diff < (epsilon * Single.MinValue);
            }
            // use relative error
            return diff / (absA + absB) < epsilon;
        }

        public static object InvokeMethodWithReflection(
            object target,
            string methodName,
            object[] parameters) {

            // Get method metadata.
            var methodInfo = target.GetType().GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Instance);

            var result = methodInfo.Invoke(target, parameters);

            return result;
        }

        public static bool QuaternionsEqual(
            Quaternion a,
            Quaternion b,
            float threshold = 0.01f) {

            var angle = Quaternion.Angle(a, b);

            return angle < threshold;
        }

        public static void RemoveAllCurveKeys(AnimationCurve curve) {
            var keysToRemoveNo = curve.length;
            for (var i = 0; i < keysToRemoveNo; i++) {
                curve.RemoveKey(0);
            }
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        ///     http://forum.unity3d.com/threads/how-to-set-an-animation-curve-to-linear-through-scripting.151683/#post-1121021
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
                    intangent = 0;
                    inTangentSet = true;
                }

                if (i == curve.keys.Length - 1) {
                    outtangent = 0;
                    outTangentSet = true;
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

        public static bool V3Equal(
            Vector3 a,
            Vector3 b,
            float precision = 0.000000000001f) {

            return Vector3.SqrMagnitude(a - b) < precision;
        }

        /// <summary>
        /// Calculate the real difference between two angles, keeping the correct sign.
        /// </summary>
        /// <remarks>http://blog.lexique-du-net.com/index.php?post/Calculate-the-real-difference-between-two-angles-keeping-the-sign</remarks>
        /// <param name="firstAngle">Old angle value.</param>
        /// <param name="secondAngle">New angle value.</param>
        /// <returns></returns>
        public static float CalculateDifferenceBetweenAngles(
            float firstAngle,
            float secondAngle) {

            float difference = secondAngle - firstAngle;

            while (difference < -180) difference += 360;
            while (difference > 180) difference -= 360;

            return difference;
        }

    }

}