using System;
using System.Linq;
using UnityEngine;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    public sealed class Shortcuts {

        #region FIELDS
        private readonly APAnimator apAnimator;

        private readonly AnimatorSettings settings;
        #endregion

        #region PROPERTIES
        public APAnimator ApAnimator {
            get { return apAnimator; }
        }

        public AnimatorSettings Settings {
            // TODO Replace with APAnimator.messageSettings.
            get { return settings; }
        }
        #endregion

        #region METHODS
        public Shortcuts(APAnimator apAnimator) {
            this.apAnimator = apAnimator;
            settings = apAnimator.Settings;
        }

        public void HandleShortcuts() {
            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.EaseModeKey,
                () => Settings.HandleMode = HandleMode.Ease);

            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.RotationModeKey,
                () => Settings.HandleMode = HandleMode.Rotation);

            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.TiltingModeKey,
                () => Settings.HandleMode = HandleMode.Tilting);

            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.NoneModeKey,
                () => Settings.HandleMode = HandleMode.None);

            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.UpdateAllKey,
                () => Settings.UpdateAllMode = !Settings.UpdateAllMode);

            // Short jump forward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        ApAnimator.AnimationTime + Settings.ShortJumpValue;

                    ApAnimator.AnimationTime =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                ApAnimator.Settings.ShortJumpForwardKey,
                Event.current.alt);

            // Short jump backward.
            Utilities.HandleModShortcut(
                () => {
                    var newAnimationTimeRatio =
                        ApAnimator.AnimationTime - Settings.ShortJumpValue;

                    ApAnimator.AnimationTime =
                        (float) (Math.Round(newAnimationTimeRatio, 3));
                },
                ApAnimator.Settings.ShortJumpBackwardKey,
                Event.current.alt);

            // Long jump forward.
            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.LongJumpForwardKey,
                () => ApAnimator.AnimationTime +=
                    ApAnimator.Settings.LongJumpValue);

            // Long jump backward.
            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.LongJumpBackwardKey,
                () => ApAnimator.AnimationTime -=
                    ApAnimator.Settings.LongJumpValue);

            // Jump to next node.
            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.JumpToNextNodeKey,
                () => ApAnimator.AnimationTime =
                    GetNearestForwardNodeTimestamp());

            // Jump to previous node.
            Utilities.HandleUnmodShortcut(
                // TODO Replace APAnimator.messageSettings with messageSettings.
                ApAnimator.Settings.JumpToPreviousNodeKey,
                () => ApAnimator.AnimationTime =
                    GetNearestBackwardNodeTimestamp());

            // Jump to start.
            Utilities.HandleModShortcut(
                () => ApAnimator.AnimationTime = 0,
                ApAnimator.Settings.JumpToStartKey,
                //ModKeyPressed);
                Event.current.alt);

            // Jump to end.
            Utilities.HandleModShortcut(
                () => ApAnimator.AnimationTime = 1,
                ApAnimator.Settings.JumpToEndKey,
                //ModKeyPressed);
                Event.current.alt);

            // Play/pause animation.
            Utilities.HandleUnmodShortcut(
                ApAnimator.Settings.PlayPauseKey,
                ApAnimator.HandlePlayPause);

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
            var pathTimestamps = ApAnimator.PathData.GetPathTimestamps();

            for (var i = pathTimestamps.Length - 1; i >= 0; i--) {
                if (pathTimestamps[i] < ApAnimator.AnimationTime) {
                    return pathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        private float GetNearestForwardNodeTimestamp() {
            var pathTimestamps = ApAnimator.PathData.GetPathTimestamps();

            foreach (var timestamp in pathTimestamps
                .Where(timestamp => timestamp > ApAnimator.AnimationTime)) {
                return timestamp;
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        #endregion
    }

}