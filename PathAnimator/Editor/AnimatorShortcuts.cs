using System;
using System.Linq;
using UnityEngine;

namespace ATP.SimplePathAnimator.Animator {

    public class AnimatorShortcuts {

        private readonly PathAnimator pathAnimator;

        private readonly AnimatorSettings settings;

        public AnimatorShortcuts(PathAnimator pathAnimator) {
            this.pathAnimator = pathAnimator;
            settings = pathAnimator.Settings;
        }

        public PathAnimator PathAnimator {
            get { return pathAnimator; }
        }

        public AnimatorSettings Settings {
            // TODO Replace with Animator.Settings.
            get { return settings; }
        }

        public void HandleShortcuts() {
            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.EaseModeKey,
                () => Settings.HandleMode = HandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.RotationModeKey,
                () => Settings.HandleMode = HandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.TiltingModeKey,
                () => Settings.HandleMode = HandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.NoneModeKey,
                () => Settings.HandleMode = HandleMode.None);

            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.UpdateAllKey,
                () => Settings.UpdateAllMode = !Settings.UpdateAllMode);

            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.MoveAllModeKey,
                ToggleMovementMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        PathAnimator.AnimationTimeRatio + Settings.ShortJumpValue;

                    PathAnimator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                PathAnimator.Settings.ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        PathAnimator.AnimationTimeRatio - Settings.ShortJumpValue;

                    PathAnimator.AnimationTimeRatio =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                PathAnimator.Settings.ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.LongJumpForwardKey,
                () => PathAnimator.AnimationTimeRatio +=
                    PathAnimator.Settings.LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.LongJumpBackwardKey,
                () => PathAnimator.AnimationTimeRatio -=
                    PathAnimator.Settings.LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.JumpToNextNodeKey,
                () => PathAnimator.AnimationTimeRatio =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                // TODO Replace Animator.Settings with Settings.
                PathAnimator.Settings.JumpToPreviousNodeKey,
                () => PathAnimator.AnimationTimeRatio =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => PathAnimator.AnimationTimeRatio = 0,
                PathAnimator.Settings.JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => PathAnimator.AnimationTimeRatio = 1,
                PathAnimator.Settings.JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                PathAnimator.Settings.PlayPauseKey,
                PathAnimator.HandlePlayPause);

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
                MovementMode.MoveSingle) {

                Settings.MovementMode = MovementMode.MoveAll;
            }
            else {
                Settings.MovementMode = MovementMode.MoveSingle;
            }
        }

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = PathAnimator.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < PathAnimator.AnimationTimeRatio) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = PathAnimator.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > PathAnimator.AnimationTimeRatio)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

    }

}