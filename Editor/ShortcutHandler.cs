using System;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public static class ShortcutHandler {

        public const KeyCode MoveAllKey = KeyCode.U;
        public const KeyCode MoveSingleModeKey = KeyCode.Y;
        public const KeyCode NoneModeShortcut = KeyCode.K;
        public const KeyCode PlayPauseShortcut = KeyCode.Space;
        public const KeyCode RotationModeShortcut = KeyCode.H;
        public const KeyCode TiltingModeShortcut = KeyCode.J;
        public const KeyCode UpdateAllShortcut = KeyCode.L;

        public const KeyCode EaseModeShortcut = KeyCode.G;

        /// <summary>
        ///     Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        ///     Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.UpArrow;

        public const KeyCode JumpToNextNode = KeyCode.UpArrow;
        public const KeyCode JumpToPreviousNode = KeyCode.DownArrow;

        /// <summary>
        ///     Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.DownArrow;

        /// <summary>
        ///     Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        private static bool modKeyPressed;

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        public static bool ModKeyPressed {
            get { return modKeyPressed; }
        }

        public static void HandleEaseModeOptionShortcut(Action callback) {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != EaseModeShortcut) return;

            callback();
        }

        public static void HandleModifiedShortcuts(
            Action jumpForwardCallback = null,
            Action jumpBackwardCallback = null,
            Action jumpToNextNodeCallback = null,
            Action jumpToPreviousNodeCallback = null,
            Action anyModJumpKeyPressedCallback = null) {

            // Check what key is pressed..
            switch (Event.current.keyCode) {
                // Jump backward.
                case JumpBackward:
                    Event.current.Use();

                    if (jumpBackwardCallback != null) jumpBackwardCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;
                // Jump forward.
                case JumpForward:
                    Event.current.Use();

                    if (jumpForwardCallback != null) jumpForwardCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;

                case JumpToNextNode:
                    Event.current.Use();

                    if (jumpToNextNodeCallback != null)
                        jumpToNextNodeCallback();
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;

                case JumpToPreviousNode:
                    Event.current.Use();

                    if (jumpToPreviousNodeCallback != null) {
                        jumpToPreviousNodeCallback();
                    }
                    if (anyModJumpKeyPressedCallback != null) {
                        anyModJumpKeyPressedCallback();
                    }

                    break;
            }
        }

        /// <summary>
        ///     Update <c>moveAllMode</c> option with keyboard shortcut.
        /// </summary>
        public static void HandleMoveAllOptionShortcut(
            Action callback) {

            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveAllKey) return;

            callback();
        }

        public static void HandleMoveSingleModeShortcut(
            Action callback) {

            // Return if Tangent Mode shortcut wasn't released.
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveSingleModeKey) return;

            callback();
        }

        public static void HandlePlayPauseShortcut(
            Action callback) {

            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != PlayPauseShortcut) return;

            callback();
        }

        public static void HandleRotationModeOptionShortcut(
            Action callback) {
            
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != RotationModeShortcut) return;

            callback();
        }

        public static void HandleTiltingModeOptionShortcut(
            Action callback) {
            
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != TiltingModeShortcut) return;

            callback();
        }

        public static void HandleUnmodifiedShortcuts(
            Action jumpBackwardCallback = null,
            Action jumpForwardCallback = null,
            Action jumpToStartCallback = null,
            Action jumpToEndCallback = null,
            Action anyJumpKeyPressedCallback = null) {

            // Helper variable.

            switch (Event.current.keyCode) {
                // Jump backward.
                case JumpBackward:
                    Event.current.Use();

                    if (jumpBackwardCallback != null) jumpBackwardCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
                // Jump forward.
                case JumpForward:
                    Event.current.Use();

                    if (jumpForwardCallback != null) jumpForwardCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
                // Jump to start.
                case JumpToStart:
                    Event.current.Use();

                    if (jumpToStartCallback != null) jumpToStartCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
                // Jump to end.
                case JumpToEnd:
                    Event.current.Use();

                    if (jumpToEndCallback != null) jumpToEndCallback();
                    if (anyJumpKeyPressedCallback != null) {
                        anyJumpKeyPressedCallback();
                    }

                    break;
            }
        }

        /// <summary>
        ///     Checked if modifier key is pressed and remember it in a class
        ///     field.
        /// </summary>
        public static void UpdateModifierKey() {
            // Check if modifier key is currently pressed.
            if (Event.current.type == EventType.keyDown
                && Event.current.keyCode == ModKey) {
                // Remember key state.
                modKeyPressed = true;
            }
            // If modifier key was released..
            if (Event.current.type == EventType.keyUp
                && Event.current.keyCode == ModKey) {
                modKeyPressed = false;
            }
        }

        public static void HandleNoneModeOptionShortcut(
            Action callback) {

            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != ShortcutHandler.NoneModeShortcut) return;

            callback();
        }

        public static void HandleUpdateAllOptionShortcut(
            Action callback) {

            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != ShortcutHandler.UpdateAllShortcut) return;

            callback();
        }

    }

}