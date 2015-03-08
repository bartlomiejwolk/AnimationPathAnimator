using System;
using System.Linq;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathAnimatorComponent {

    public sealed class Shortcuts {

        #region FIELDS
        private readonly PathAnimator pathAnimator;

        private readonly PathAnimatorSettings settings;
        #endregion

        #region PROPERTIES
        public PathAnimator PathAnimator {
            get { return pathAnimator; }
        }

        public PathAnimatorSettings Settings {
            // TODO Replace with Animator.Settings.
            get { return settings; }
        }
        #endregion

        #region METHODS
        public Shortcuts(PathAnimator pathAnimator) {
            this.pathAnimator = pathAnimator;
            settings = pathAnimator.Settings;
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

        private float GetNearestBackwardNodeTimestamp() {
            var pathTimestamps = PathAnimator.PathData.GetPathTimestamps();

            // Get decimal part of the timestamp.
            var baseTime = Mathf.Floor(PathAnimator.AnimationTimeRatio);

            // Increase timestamp value by the decimal base.
            for (int i = 0; i < pathTimestamps.Length; i++) {
                pathTimestamps[i] += baseTime;
            }

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                //pathTimestamps[i] += baseTime;

                if (pathTimestamps[i] < PathAnimator.AnimationTimeRatio) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = PathAnimator.PathData.GetPathTimestamps();

            // Get decimal part of the timestamp.
            var baseTime = Mathf.Floor(PathAnimator.AnimationTimeRatio);

            // Increase timestamp value by the decimal base.
            for (int i = 0; i < pathTimestamps.Length; i++) {
                pathTimestamps[i] += baseTime;
            }

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > PathAnimator.AnimationTimeRatio)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return baseTime + 1.0f;
        }

        #endregion
    }

}