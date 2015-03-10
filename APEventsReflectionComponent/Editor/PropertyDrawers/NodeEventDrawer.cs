using UnityEngine;
using System.Collections;
using UnityEditor;

namespace ATP.AnimationPathAnimator.APEventsReflectionComponent {

    [CustomPropertyDrawer(typeof(NodeEvent))]
    public sealed class NodeEventDrawer : PropertyDrawer {

        // How many rows (properties) will be displayed.
        private const int Rows = 4;

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

            SerializedProperty sourceGO =
                prop.FindPropertyRelative("sourceGO");

            SerializedProperty sourceMethodIndex =
                prop.FindPropertyRelative("sourceMethodIndex");

            //SerializedProperty sourceComponents =
            //    prop.FindPropertyRelative("sourceComponents");

            SerializedProperty methodArg =
                prop.FindPropertyRelative("methodArg");

            EditorGUIUtility.labelWidth = 55;

            EditorGUI.PropertyField(
                    new Rect(pos.x, pos.y, pos.width, PropHeight),
                    sourceGO,
                    new GUIContent("Source GO", ""));

            // If source GO is assigned..
            if (sourceGO.objectReferenceValue != null) {
                // Get reference to source GO.
                var sourceGORef = sourceGO.objectReferenceValue as GameObject;
                // Get source game object components.
                var sourceComponents = sourceGORef.GetComponents<Component>();
                // Initialize array for source GO component names.
                var sourceCoNames= new string[sourceComponents.Length];
                // Fill array with component names.
                for (int i = 0; i < sourceCoNames.Length; i++) {
                    sourceCoNames[i] = sourceComponents[i].GetType().ToString();
                }
                // Display dropdown game object component list.
                sourceMethodIndex.intValue = EditorGUI.Popup(
                     new Rect(
                        pos.x,
                        pos.y + 1 * (PropHeight + PropMargin),
                        pos.width,
                        PropHeight), 
                    "Source Component",
                    sourceMethodIndex.intValue,
                    sourceCoNames);
            }

            //EditorGUI.PropertyField(
            //        new Rect(
            //            pos.x,
            //            pos.y + 1 * (PropHeight + PropMargin),
            //            pos.width,
            //            PropHeight),
            //        methodArg,
            //        new GUIContent("Arg.", ""));
        }
    }
}
