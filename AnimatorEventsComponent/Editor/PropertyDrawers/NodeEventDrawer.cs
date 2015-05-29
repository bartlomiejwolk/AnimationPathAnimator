// Copyright (c) 2015 Bartlomiej Wolk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace AnimationPathAnimator.AnimatorEventsComponent {

    [CustomPropertyDrawer(typeof (NodeEventSlot))]
    public sealed class NodeEventDrawer : PropertyDrawer {

        // Hight of a single property.
        private const int PropHeight = 16;
        // Margin between properties.
        private const int PropMargin = 4;
        // Space between rows.
        private const int RowsSpace = 8;
        // Overall hight of the serialized property.
        public override float GetPropertyHeight(
            SerializedProperty property,
            GUIContent label) {

            // Property with number of rows to be displayed.
            var rowsProperty = property.FindPropertyRelative("rows");
            // Copy rows number to local variable.
            var rows = rowsProperty.intValue;

            // Calculate property height.
            return base.GetPropertyHeight(property, label)
                   * rows // Each row is 16 px high.
                   + (rows - 1) * RowsSpace;
        }

        public override void OnGUI(
            Rect pos,
            SerializedProperty prop,
            GUIContent label) {

            var rowsProperty =
                prop.FindPropertyRelative("rows");

            var sourceGO =
                prop.FindPropertyRelative("sourceGO");

            var sourceCo =
                prop.FindPropertyRelative("sourceCo");

            var sourceComponentIndex =
                prop.FindPropertyRelative("sourceComponentIndex");

            var sourceMethodIndex =
                prop.FindPropertyRelative("sourceMethodIndex");

            var sourceMethodName =
                prop.FindPropertyRelative("sourceMethodName");

            var methodArg =
                prop.FindPropertyRelative("methodArg");

            EditorGUIUtility.labelWidth = 80;

            // Draw source GO field.
            var sourceGOChanged = DrawSourceGOField(pos, sourceGO);

            // If source GO was changed, reset component index to avoid null
            // ref. exception.
            if (sourceGOChanged) sourceComponentIndex.intValue = 0;

            // If source GO is not assigned..
            if (sourceGO.objectReferenceValue == null) {
                // Set rows number to 1.
                rowsProperty.intValue = 1;

                sourceMethodName.stringValue = "";

                return;
            }
            // Set rows number to 4.
            rowsProperty.intValue = 4;

            // Get reference to source GO.
            var sourceGORef = sourceGO.objectReferenceValue as GameObject;
            // Get source game object components.
            var sourceComponents = sourceGORef.GetComponents<Component>();
            // Initialize array for source GO component names.
            var sourceCoNames = new string[sourceComponents.Length];
            // Fill array with component names.
            for (var i = 0; i < sourceCoNames.Length; i++) {
                sourceCoNames[i] = sourceComponents[i].GetType().ToString();
            }
            // Make sure that current name index corresponds to a component.
            // Important when changing source game object.
            if (sourceComponentIndex.intValue > sourceCoNames.Length - 1) {
                sourceComponentIndex.intValue = 0;
            }

            // Draw source component dropdown.
            var sourceComponentChanged =
                DrawSourceComponentDropdown(
                    pos,
                    sourceComponentIndex,
                    sourceCoNames);

            // If source component was changed, reset method names index to
            // avoid null ref. exception.
            if (sourceComponentChanged) {
                sourceMethodIndex.intValue = 0;
            }

            // Update source component ref. in the NodeEventSlot property.
            sourceCo.objectReferenceValue =
                sourceComponents[sourceComponentIndex.intValue];

            // Get target component method names.
            var methods = sourceComponents[sourceComponentIndex.intValue]
                .GetType()
                .GetMethods(
                    BindingFlags.Instance | BindingFlags.Static
                    | BindingFlags.Public | BindingFlags.DeclaredOnly);
            // Initialize array with method names.
            var methodNames = new string[methods.Length];
            // Fill array with method names.
            for (var i = 0; i < methodNames.Length; i++) {
                methodNames[i] = methods[i].Name;
            }

            DrawMethodNamesDropdown(pos, sourceMethodIndex, methodNames);

            // Update method name in the NodeEventSlot property.
            sourceMethodName.stringValue =
                methodNames[sourceMethodIndex.intValue];

            DrawMethodArgumentField(pos, sourceGO, methodArg);
        }

        private static void DrawMethodArgumentField(
            Rect pos,
            SerializedProperty sourceGO,
            SerializedProperty methodArg) {

            // Don't draw parameter field if source GO is not specified.
            if (sourceGO.objectReferenceValue == null) return;
            EditorGUI.PropertyField(
                new Rect(
                    pos.x,
                    pos.y + 3 * (PropHeight + PropMargin),
                    pos.width,
                    PropHeight),
                methodArg,
                new GUIContent("Argument", ""));
        }

        private static void DrawMethodNamesDropdown(
            Rect pos,
            SerializedProperty sourceMethodIndex,
            string[] methodNames) {

            // Display dropdown with component properties.
            sourceMethodIndex.intValue = EditorGUI.Popup(
                new Rect(
                    pos.x,
                    pos.y + 2 * (PropHeight + PropMargin),
                    pos.width,
                    PropHeight),
                "Methods",
                sourceMethodIndex.intValue,
                methodNames);
        }

        private static bool DrawSourceComponentDropdown(
            Rect pos,
            SerializedProperty sourceComponentIndex,
            string[] sourceCoNames) {

            var prevIndex = sourceComponentIndex.intValue;

            // Display dropdown game object component list.
            sourceComponentIndex.intValue = EditorGUI.Popup(
                new Rect(
                    pos.x,
                    pos.y + 1 * (PropHeight + PropMargin),
                    pos.width,
                    PropHeight),
                "Source Component",
                sourceComponentIndex.intValue,
                sourceCoNames);

            if (sourceComponentIndex.intValue != prevIndex) {
                return true;
            }

            return false;
        }

        private static bool DrawSourceGOField(
            Rect pos,
            SerializedProperty sourceGO) {

            var prevSourceGO = sourceGO.objectReferenceValue;

            EditorGUI.PropertyField(
                new Rect(pos.x, pos.y, pos.width, PropHeight),
                sourceGO,
                new GUIContent("Source GO", ""));

            if (sourceGO.objectReferenceValue != prevSourceGO) {
                return true;
            }

            return false;
        }

    }

}