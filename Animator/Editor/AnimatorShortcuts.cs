using System;
using System.Linq;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

    public class AnimatorShortcuts {

        private readonly Animator animator;

        private readonly AnimatorSettings settings;

        public AnimatorShortcuts(Animator animator) {
            this.animator = animator;
            settings = animator.Settings;
        }

        public Animator Animator {
            get { return animator; }
        }

        public AnimatorSettings Settings {
            // TODO Replace with Animator.Settings.
            get { return settings; }
        }

        public void HandleShortcuts() {
            Utilities.HandleUnmodShortcut(
                Animator.Settings.EaseModeKey,
                () => Settings.HandleMode = AnimatorHandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                Animator.Settings.RotationModeKey,
                () => Settings.HandleMode = AnimatorHandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                Animator.Settings.TiltingModeKey,
                () => Settings.HandleMode = AnimatorHandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                Animator.Settings.NoneModeKey,
                () => Settings.HandleMode = AnimatorHandleMode.None);

            Utilities.HandleUnmodShortcut(
                Animator.Settings.UpdateAllKey,
                () => Settings.UpdateAllMode = !Settings.UpdateAllMode);

            Utilities.HandleUnmodShortcut(
                Animator.Settings.MoveAllModeKey,
                ToggleMovementMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        Animator.AnimationTimeRatio + Settings.ShortJumpValue;

                    Animator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                Animator.Settings.ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        Animator.AnimationTimeRatio - Settings.ShortJumpValue;

                    Animator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                Animator.Settings.ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(
                Animator.Settings.LongJumpForwardKey,
                () => Animator.AnimationTimeRatio +=
                    Animator.Settings.LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(
                Animator.Settings.LongJumpBackwardKey,
                () => Animator.AnimationTimeRatio -=
                    Animator.Settings.LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                Animator.Settings.JumpToNextNodeKey,
                () => Animator.AnimationTimeRatio =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                // TODO Replace Animator.Settings with Settings.
                Animator.Settings.JumpToPreviousNodeKey,
                () => Animator.AnimationTimeRatio =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => Animator.AnimationTimeRatio = 0,
                Animator.Settings.JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => Animator.AnimationTimeRatio = 1,
                Animator.Settings.JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                Animator.Settings.PlayPauseKey,
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
            if (Settings.MovementMode ==
                AnimationPathBuilderHandleMode.MoveSingle) {

                Settings.MovementMode = AnimationPathBuilderHandleMode.MoveAll;
            }
            else {
                Settings.MovementMode = AnimationPathBuilderHandleMode.MoveSingle;
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