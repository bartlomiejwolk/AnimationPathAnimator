using System;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public class ShortcutHandler {

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
        private bool modKeyPressed;

        private AnimationPathAnimator animator;

        public ShortcutHandler(AnimationPathAnimator animator) {
            this.animator = animator;
        }

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        public bool ModKeyPressed {
            get { return modKeyPressed; }
        }

        public void HandleEaseModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != EaseModeShortcut) return;

            animator.HandleMode = AnimatorHandleMode.Ease;
        }

        public void HandleModifiedShortcuts(
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
        public void HandleMoveAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveAllKey) return;

            animator.MovementMode = AnimationPathBuilderHandleMode.MoveAll;
        }

        public void HandleMoveSingleModeShortcut() {
            // Return if Tangent Mode shortcut wasn't released.
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != MoveSingleModeKey) return;

            animator.MovementMode = AnimationPathBuilderHandleMode.MoveSingle;
        }

        public void HandlePlayPauseShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != PlayPauseShortcut) return;

            animator.HandlePlayPause();
        }

        public void HandleRotationModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != RotationModeShortcut) return;

            animator.HandleMode = AnimatorHandleMode.Rotation;
        }

        public void HandleTiltingModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != TiltingModeShortcut) return;

            animator.HandleMode = AnimatorHandleMode.Tilting;
        }

        public void HandleUnmodifiedShortcuts(
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
        public void UpdateModifierKey() {
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

        public void HandleNoneModeOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != ShortcutHandler.NoneModeShortcut) return;

            animator.HandleMode = AnimatorHandleMode.None;
        }

        public void HandleUpdateAllOptionShortcut() {
            if (Event.current.type != EventType.keyUp
                || Event.current.keyCode != ShortcutHandler.UpdateAllShortcut) return;

            animator.UpdateAllMode = !animator.UpdateAllMode;
        }

    }

}