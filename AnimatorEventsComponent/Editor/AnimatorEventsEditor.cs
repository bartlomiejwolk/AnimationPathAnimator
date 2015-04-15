// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
//  
// This file is part of the AnimationPath Animator Unity extension.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using System.Collections.Generic;
using AnimationPathAnimator.AnimatorComponent;
using AnimationPathAnimator.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace AnimationPathAnimator.AnimatorEventsComponent {

    [CustomEditor(typeof (AnimatorEvents))]
    public class AnimatorEventsEditor : Editor {
        #region PROPERTIES

        public bool SerializedPropertiesInitialized { get; set; }

        private AnimatorEvents Script { get; set; }

        private AnimatorEventsSettings Settings { get; set; }

        #endregion PROPERTIES

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animator;
        private SerializedProperty drawMethodNames;
        private SerializedProperty nodeEvents;
        private SerializedProperty settings;
        private SerializedProperty skin;

        #endregion SERIALIZED PROPERTIES

        #region UNITY MESSAGES

        public override void OnInspectorGUI() {
            if (!AssetsLoaded()) {
                DrawInfoLabel(
                    "Required assets were not found.\n"
                    + "Reset component and if it does not help, restore extension "
                    + "folder content to its default state.");
                return;
            }
            if (!SerializedPropertiesInitialized) return;

            DrawAnimatorField();

            EditorGUILayout.Space();

            DisplayDrawMethodLabelsToggle();

            DrawReorderableEventList();

            DrawAdvancedSettingsFoldout();
            DrawAdvancedSettingsControls();
        }

        private void OnEnable() {
            Script = target as AnimatorEvents;

            if (!AssetsLoaded()) return;

            Settings = Script.Settings;

            InitializeSerializedProperties();
        }

        private void OnSceneGUI() {
            if (!AssetsLoaded()) return;

            HandleDrawingMethodNames();
        }

        #endregion UNITY MESSAGES

        #region INSPECTOR

        private void DisplayDrawMethodLabelsToggle() {
            serializedObject.Update();
            EditorGUILayout.PropertyField(
                drawMethodNames,
                new GUIContent(
                    "Draw Labels",
                    "Draw on-scene label for each event handling method."));
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAdvancedSettingsControls() {
            if (advancedSettingsFoldout.boolValue) {
                DrawSettingsAssetField();
                DrawSkinAssetField();
            }
        }

        private void DrawAdvancedSettingsFoldout() {
            serializedObject.Update();

            advancedSettingsFoldout.boolValue = EditorGUILayout.Foldout(
                advancedSettingsFoldout.boolValue,
                new GUIContent(
                    "Advanced Settings",
                    ""));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawAnimatorField() {

            EditorGUILayout.PropertyField(
                animator,
                new GUIContent(
                    "Animator",
                    "Animator component reference."));
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawReorderableEventList() {
            serializedObject.Update();

            ReorderableListGUI.Title("AnimatorEvents");
            ReorderableListGUI.ListField(
                nodeEvents,
                ReorderableListFlags.HideAddButton
                | ReorderableListFlags.HideRemoveButtons
                | ReorderableListFlags.DisableContextMenu
                | ReorderableListFlags.ShowIndices);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    "Reference to asset with all AnimatorEvents component settings."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSkinAssetField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(skin);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion INSPECTOR

        #region METHODS

        private bool AssetsLoaded() {
            return (bool) Utilities.InvokeMethodWithReflection(
                Script,
                "RequiredAssetsLoaded",
                null);
        }

        private void HandleDrawingMethodNames() {
            if (!drawMethodNames.boolValue) return;
            // Return if path data does not exist.
            if (Script.Animator.PathData == null) return;

            var methodNames = (string[]) Utilities.InvokeMethodWithReflection(
                Script,
                "GetMethodNames",
                null);

            var nodePositions =
                (List<Vector3>) Utilities.InvokeMethodWithReflection(
                    Script,
                    "GetNodePositions",
                    new object[] {-1});

            // Wait until event slots number is synced with path nodes number.
            if (methodNames.Length != nodePositions.Count) return;

            var style = Script.Skin.GetStyle("MethodNameLabel");

            SceneHandles.DrawNodeLabels(
                nodePositions,
                methodNames,
                Settings.MethodNameLabelOffsetX,
                Settings.MethodNameLabelOffsetY,
                Settings.DefaultNodeLabelWidth,
                Settings.DefaultNodeLabelHeight,
                style);
        }

        private void InitializeSerializedProperties() {
            animator =
                serializedObject.FindProperty("animator");
            advancedSettingsFoldout =
                serializedObject.FindProperty("advancedSettingsFoldout");
            skin =
                serializedObject.FindProperty("skin");
            settings =
                serializedObject.FindProperty("settings");
            nodeEvents = serializedObject.FindProperty("nodeEventSlots");
            drawMethodNames =
                serializedObject.FindProperty("drawMethodNames");

            SerializedPropertiesInitialized = true;
        }

        #endregion METHODS
    }

}