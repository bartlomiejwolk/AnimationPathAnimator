using System;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public class AnimatorShortcuts {

        private readonly AnimationPathAnimator animator;

        public AnimatorShortcuts(AnimationPathAnimator animator) {
            this.animator = animator;
        }

        // TODO Make protected instead of public.
        public virtual KeyCode EaseModeKey {
            get { return KeyCode.U; }
        }

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        public virtual KeyCode JumpToEndKey {
            get { return KeyCode.L; }
        }

        public virtual KeyCode JumpToNextNodeKey {
            get { return KeyCode.L; }
        }

        public virtual KeyCode JumpToPreviousNodeKey {
            get { return KeyCode.H; }
        }

        public virtual KeyCode JumpToStartKey {
            get { return KeyCode.H; }
        }

        public KeyCode LongJumpBackwardKey {
            get { return KeyCode.J; }
        }

        public KeyCode LongJumpForwardKey {
            get { return KeyCode.K; }
        }

        public virtual float LongJumpValue {
            get { return 0.01f; }
        }

        public virtual KeyCode ModKey {
            get { return KeyCode.RightAlt; }
        }

        public virtual Color MoveAllModeColor {
            get { return Color.red; }
        }

        public virtual KeyCode MoveAllModeKey {
            get { return KeyCode.P; }
        }

        //public virtual KeyCode MoveSingleModeKey {
        //    get { return KeyCode.Y; }
        //}

        public virtual KeyCode NoneModeKey {
            get { return KeyCode.Y; }
        }

        public virtual KeyCode PlayPauseKey {
            get { return KeyCode.Space; }
        }

        public virtual KeyCode RotationModeKey {
            get { return KeyCode.I; }
        }

        public KeyCode ShortJumpBackwardKey {
            get { return KeyCode.J; }
        }

        public KeyCode ShortJumpForwardKey {
            get { return KeyCode.K; }
        }

        public virtual KeyCode TiltingModeKey {
            get { return KeyCode.O; }
        }

        public virtual KeyCode UpdateAllKey {
            get { return KeyCode.G; }
        }

        public AnimationPathAnimator Animator {
            get { return animator; }
        }

        public void HandleShortcuts() {
            Utilities.HandleUnmodShortcut(
                EaseModeKey,
                () => Animator.HandleMode = AnimatorHandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                RotationModeKey,
                () => Animator.HandleMode = AnimatorHandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                TiltingModeKey,
                () => Animator.HandleMode = AnimatorHandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                NoneModeKey,
                () => Animator.HandleMode = AnimatorHandleMode.None);

            Utilities.HandleUnmodShortcut(
                UpdateAllKey,
                () => Animator.UpdateAllMode = !Animator.UpdateAllMode);

            Utilities.HandleUnmodShortcut(
                MoveAllModeKey,
                ToggleMovementMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio = Animator.AnimationTimeRatio + Animator.ShortJumpValue;

                    Animator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                }, ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio = Animator.AnimationTimeRatio - Animator.ShortJumpValue;

                    Animator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                }, ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(LongJumpForwardKey,
                () => Animator.AnimationTimeRatio += LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(LongJumpBackwardKey,
                () => Animator.AnimationTimeRatio -= LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                JumpToNextNodeKey,
                () => Animator.AnimationTimeRatio =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                JumpToPreviousNodeKey,
                () => Animator.AnimationTimeRatio =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => Animator.AnimationTimeRatio = 0,
                JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => Animator.AnimationTimeRatio = 1,
                JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                PlayPauseKey,
                Animator.HandlePlayPause);

            //if (Event.current.type == EventType.keyDown
            //    //&& Event.current.keyCode == KeyCode.C) {
            //    && Event.current.keyCode == KeyCode.C
            //    //&& Event.current.modifiers == EventModifiers.Alt) {
            //    && Event.current.alt) {

            //    //Event.current.Use();
            //    //Debug.Log(Event.current.modifiers);
            //    Debug.Log("Alt + C");
            //}
        }
        private void ToggleMovementMode() {
            if (Animator.MovementMode ==
                AnimationPathBuilderHandleMode.MoveSingle) {

                Animator.MovementMode = AnimationPathBuilderHandleMode.MoveAll;
            }
            else {
                Animator.MovementMode = AnimationPathBuilderHandleMode.MoveSingle;
            }
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = Animator.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < Animator.AnimationTimeRatio) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = Animator.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > Animator.AnimationTimeRatio)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

    }

}