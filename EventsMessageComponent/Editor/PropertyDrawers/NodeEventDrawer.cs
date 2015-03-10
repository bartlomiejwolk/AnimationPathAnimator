using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ATP.AnimationPathAnimator.EventsMessageComponent {

    [CustomPropertyDrawer(typeof(NodeEvent))]
    public sealed class NodeEventDrawer : PropertyDrawer {

        // How many rows (properties) will be displayed.
        private const int Rows = 3;

        // Hight of a single property.
        private const int PropHeight = 16;

        // Margin between properties.
        private const int PropMargin = 4;

        private const int RowsSpace = 8;

        // Overall hight of the serialized property.
        public override float GetPropertyHeight(
                SerializedProperty property,
                GUIContent label) {

            return base.GetPropertyHeight(property, label)
                * Rows // Each row is 16 px high.
                + (Rows - 1) * RowsSpace; // Add 4 px for each spece between rows.
        }

        public override void OnGUI(
                Rect pos,
                SerializedProperty prop,
                GUIContent label) {

            SerializedProperty methodName =
                prop.FindPropertyRelative("methodName");

            SerializedProperty methodArg =
                prop.FindPropertyRelative("methodArg");

            EditorGUIUtility.labelWidth = 55;

            EditorGUI.PropertyField(
                    new Rect(pos.x, pos.y, pos.width, PropHeight),
                    methodName,
                    new GUIContent("Method", ""));

            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 1 * (PropHeight + PropMargin),
                        pos.width,
                        PropHeight),
                    methodArg,
                    new GUIContent("Arg.", ""));
        }
    }
}
