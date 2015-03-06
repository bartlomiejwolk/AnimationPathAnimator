using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ATP.SimplePathAnimator.Events {

    [CustomPropertyDrawer(typeof(NodeEvent))]
    public sealed class NodeEventDrawer : PropertyDrawer {

        // How many rows (properties) will be displayed.
        private static int Rows {
            get { return 2; }
        }

        // Hight of a single property.
        private static int PropHeight {
            get { return 16; }
        }

        // Margin between properties.
        private static int PropMargin {
            get { return 4; }
        }

        private static int RowsSpace {
            get { return 8; }
        }

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
