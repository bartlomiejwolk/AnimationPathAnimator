using System;
using UnityEngine;
using System.Collections;
using UnityEditor;
using ATP.ReorderableList;

namespace ATP.AnimationPathTools {

    [CustomEditor(typeof(AnimationPathAnimator))]
	public class AnimatorEditor: Editor {

        /// <summary>
        /// Reference to target script.
        /// </summary>
		private AnimationPathAnimator script;

        // Serialized properties
		private SerializedProperty duration;
		private SerializedProperty animTimeRatio;
		private SerializedProperty animations;
        private SerializedProperty _easeAnimationCurve;

        /// <summary>
        ///     If modifier is currently pressed.
        /// </summary>
        private bool modKeyPressed;

		void OnEnable() {
            // Get target script reference.
			script = (AnimationPathAnimator)target;

            // Initialize serialized properties.
			duration = serializedObject.FindProperty("duration");
			animTimeRatio = serializedObject.FindProperty("animTimeRatio");
			animations = serializedObject.FindProperty("animations");
		    _easeAnimationCurve = serializedObject.FindProperty("_easeAnimationCurve");
		}

		public override void OnInspectorGUI() {
			serializedObject.Update();

			EditorGUILayout.Slider(
					animTimeRatio,
					0,
					1);

			EditorGUILayout.PropertyField(duration);

		    EditorGUILayout.PropertyField(
		        _easeAnimationCurve,
		        new GUIContent(
		            "Ease Curve",
		            ""));

			ReorderableListGUI.Title("Animations");
			ReorderableListGUI.ListField(animations);

			// Save changes
			serializedObject.ApplyModifiedProperties();

            // TODO Delete if not needed.
			if (GUI.changed) {
				EditorUtility.SetDirty(script);
			}
		}

		void OnSceneGUI() {
			serializedObject.Update();

            // Update modifier key state.
			UpdateModifierKey();

            // Change current animation time with arrow keys.
			ChangeTimeWithArrowKeys();

			// Save changes
			serializedObject.ApplyModifiedProperties();

		    script.UpdateAnimation();
		}

        /// <summary>
        /// Change current animation time with arrow keys.
        /// </summary>
        // TODO Refactor.
		private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
			if (Event.current.type == EventType.keyDown
                    // and modifier key is pressed also..
					&& modKeyPressed) {

                // Check what key is pressed..
				switch (Event.current.keyCode) {
                    // Jump backward.
					case AnimationPathAnimator.JumpBackward:
						Event.current.Use();

                        // Update animation time.
						animTimeRatio.floatValue -=
                            AnimationPathAnimator.JumpValue;

						break;
                    // Jump forward.
					case AnimationPathAnimator.JumpForward:
						Event.current.Use();

                        // Update animation time.
						animTimeRatio.floatValue +=
                            AnimationPathAnimator.JumpValue;

						break;
                    case AnimationPathAnimator.JumpToStart:
                        Event.current.Use();

                        // Jump to next node.
				        animTimeRatio.floatValue = GetNearestNodeForwardTimestamp();

				        break;
                    case AnimationPathAnimator.JumpToEnd:
                        Event.current.Use();

                        // Jump to next node.
                        animTimeRatio.floatValue = GetNearestNodeBackwardTimestamp();

                        break;
				}
            }
			// Modifier key not pressed.
			else if (Event.current.type == EventType.keyDown) {
                // Helper variable.
			    float newAnimationTimeRatio;
				switch (Event.current.keyCode) {
                    // Jump backward.
					case AnimationPathAnimator.JumpBackward:
						Event.current.Use();

                        // Calculate new time ratio.
				        newAnimationTimeRatio = animTimeRatio.floatValue
                            - AnimationPathAnimator.ShortJumpValue;
                        // Apply rounded value.
				        animTimeRatio.floatValue =
                            (float)(Math.Round(newAnimationTimeRatio, 3));

						break;
                    // Jump forward.
					case AnimationPathAnimator.JumpForward:
						Event.current.Use();

				        newAnimationTimeRatio = animTimeRatio.floatValue
                            + AnimationPathAnimator.ShortJumpValue;
				        animTimeRatio.floatValue =
                            (float)(Math.Round(newAnimationTimeRatio, 3));

						break;
				}
			}

			// Handle up/down arrows.
			if (Event.current.type == EventType.keyDown) {
				switch (Event.current.keyCode) {
					case AnimationPathAnimator.JumpToStart:
						Event.current.Use();

						animTimeRatio.floatValue = 1;

						break;
					case AnimationPathAnimator.JumpToEnd:
						Event.current.Use();

						animTimeRatio.floatValue = 0;

						break;
				}
			}
		}

        private float GetNearestNodeForwardTimestamp() {
            float[] targetPathTimestamps = script.GetTargetPathTimestamps();

            for (var i = 0; i < targetPathTimestamps.Length; i++) {
                if (targetPathTimestamps[i] > animTimeRatio.floatValue) {
                    return targetPathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 1.0f;
        }

        private float GetNearestNodeBackwardTimestamp() {
            float[] targetPathTimestamps = script.GetTargetPathTimestamps();

            for (var i = targetPathTimestamps.Length - 1; i >= 0; i--) {
                if (targetPathTimestamps[i] < animTimeRatio.floatValue) {
                    return targetPathTimestamps[i];
                }
            }

            // Return timestamp of the last node.
            return 0;
        }

        /// <summary>
        ///     Checked if modifier key is pressed and remember it in a class
        ///     field.
        /// </summary>
        private void UpdateModifierKey() {
            // Check if modifier key is currently pressed.
            if (Event.current.type == EventType.keyDown
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                // Remember key state.
                modKeyPressed = true;
            }
            // If modifier key was released..
            if (Event.current.type == EventType.keyUp
                    && Event.current.keyCode == AnimationPathAnimator.ModKey) {

                modKeyPressed = false;
            }
        }
	}
}
