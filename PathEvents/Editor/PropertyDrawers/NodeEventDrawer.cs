using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ATP.SimplePathAnimator.PathEvents {

    [CustomPropertyDrawer(typeof(NodeEvent))]
    public class NodeEventDrawer : PropertyDrawer {

        /// How many rows (properties) will be displayed.
        const int _rows = 2;
        /// Hight of a single property.
        const int propHeight = 16;
        /// Margin between properties.
        const int propMargin = 4;
        const int RowsSpace = 8;

        /// Overall hight of the serialized property.
        public override float GetPropertyHeight(
                SerializedProperty property,
                GUIContent label) {

            return base.GetPropertyHeight(property, label)
                * _rows // Each row is 16 px high.
                + (_rows - 1) * RowsSpace; // Add 4 px for each spece between rows.
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
                    new Rect(pos.x, pos.y, pos.width, propHeight),
                    methodName,
                    new GUIContent("Method", ""));

            EditorGUI.PropertyField(
                    new Rect(
                        pos.x,
                        pos.y + 1 * (propHeight + propMargin),
                        pos.width,
                        propHeight),
                    methodArg,
                    new GUIContent("Arg.", ""));
        }
    }
}
