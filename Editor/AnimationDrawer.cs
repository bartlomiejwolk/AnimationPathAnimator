using UnityEngine;
using System.Collections;
using UnityEditor;
using OneDayGame.LoggingTools;

namespace OneDayGame.AnimationPathTools {

	[CustomPropertyDrawer(typeof(Animation))]
	public class AnimationDrawer : PropertyDrawer {

        /// <summary>
        ///     How many rows (properties) will be displayed.
        /// </summary>
		const int _rows = 6;

        /// <summary>
        ///     Hight of a single property.
        /// </summary>
        const int propHeight = 16;

        /// <summary>
        ///     Margin between properties.
        /// </summary>
        const int propMargin = 4; 

		public override float GetPropertyHeight(
				SerializedProperty property,
				GUIContent label) {

			return base.GetPropertyHeight(property, label)
				* _rows // Each row is 16 px high.
				+ (_rows - 1) * 4; // Add 4 px for each spece between rows.
		}

		public override void OnGUI(
				Rect pos,
				SerializedProperty prop,
				GUIContent label) {

            SerializedProperty target =
                prop.FindPropertyRelative("target");
            SerializedProperty path =
                prop.FindPropertyRelative("path");
            SerializedProperty lookAtTarget =
                prop.FindPropertyRelative("lookAtTarget");
            SerializedProperty lookAtPath =
                prop.FindPropertyRelative("lookAtPath");

            EditorGUIUtility.labelWidth = 60;
            EditorGUI.HelpBox(
                    new Rect(pos.x, pos.y, pos.width, propHeight),
                    "Object to animate.",
                    UnityEditor.MessageType.Info);
            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 1 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    target,
                    new GUIContent("Target", ""));
            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 2 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    path,
                    new GUIContent("Path", ""));
            EditorGUI.HelpBox(
                    new Rect(
                        pos.x,
                        pos.y + 3 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    "Target to look at",
                    UnityEditor.MessageType.Info);
            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 4 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    lookAtTarget,
                    new GUIContent(
                        "Target",
                        "Transform to look at"));
            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 5 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    lookAtPath,
                    new GUIContent(
                        "Path",
                        "Path for LookAt() target"));
		}
	}
}
