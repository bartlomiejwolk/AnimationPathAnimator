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
		private void ChangeTimeWithArrowKeys() {
            // If a key is pressed..
			if (Event.current.type == EventType.keyDown
                    // and modifier key is pressed also..
					&& modKeyPressed == true) {

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
				}
            }
			// Modifier key not pressed.
			else if (Event.current.type == EventType.keyDown) {
				switch (Event.current.keyCode) {
                    // Jump backward.
					case AnimationPathAnimator.JumpBackward:
						Event.current.Use();

                        animTimeRatio.floatValue -=
                            AnimationPathAnimator.ShortJumpValue;

						break;
                    // Jump forward.
					case AnimationPathAnimator.JumpForward:
						Event.current.Use();

						animTimeRatio.floatValue +=
                            AnimationPathAnimator.ShortJumpValue;

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
