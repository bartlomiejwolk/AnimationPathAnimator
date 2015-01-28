using OneDayGame.LoggingTools;
using System;
using UnityEngine;

namespace OneDayGame.AnimationPathTools {

    /// <summary>
    ///     Static methods used across all <c>AnimationPathTools</c> classes.
    /// </summary>
    public static class AnimationPathUtilities {

        /// <summary>
        ///     Check if key exists at timestamp in a given animation curve.
        /// </summary>
        /// <param name="curve">
        ///     Animation curve.
        /// </param>
        /// <param name="time">
        ///     Animation curve timestamp.
        /// </param>
        /// <returns>
        ///     True if key at the timestamp exists.
        /// </returns>
        public static bool KeyAtTimeExists(AnimationCurve curve, float time) {
            // For each key in the curve..
            foreach (Keyframe key in curve.keys) {
                // Check if its time is the same as time argument.
                if (key.time == time) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        ///     Check if modifier key is pressed and remember that in a class
        ///     field.
        /// </summary>
        public static void UpdateKeyboardKey(
            KeyCode keyCode,
            ref bool keyPressed,
            ref bool keyPressedInPreviousFrame) {

            // Modifier key just pressed.
            if (Event.current.type == EventType.keyDown
                    && Event.current.keyCode == keyCode
                    && keyPressed == false
                    && keyPressedInPreviousFrame == false) {

                // Remember key state.
                keyPressed = true;
            }
            // Modifier key hold.
            else if (keyPressed == true
                    && keyPressedInPreviousFrame == false) {

                keyPressedInPreviousFrame = true;
            }
            // Modifier key just released..
            else if (Event.current.type == EventType.keyUp
                    && Event.current.keyCode == keyCode) {

                keyPressed = false;
                keyPressedInPreviousFrame = true;
            }
            // Modifier key hold.
            else if (keyPressed == true
                    && keyPressedInPreviousFrame == true) {

                keyPressedInPreviousFrame = true;
            }
            // Key not pressed.
            else if (keyPressed == false
                    && keyPressedInPreviousFrame == true) {

                keyPressedInPreviousFrame = false;
            }
        }
    }
}