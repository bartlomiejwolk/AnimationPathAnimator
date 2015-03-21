using ATP.AnimationPathTools.AnimatorComponent;
using ATP.AnimationPathTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.EventsComponent {

    [CustomEditor(typeof (APEvents))]
    public class APEventsEditor : Editor {

        #region PROPERTIES

        public bool SerializedPropertiesInitialized { get; set; }

        private APEvents Script { get; set; }

        private APEventsSettings Settings { get; set; }

        #endregion

        #region SERIALIZED PROPERTIES

        private SerializedProperty advancedSettingsFoldout;

        private SerializedProperty animator;
        private SerializedProperty drawMethodNames;
        private SerializedProperty nodeEvents;
        private SerializedProperty settings;
        private SerializedProperty skin;

        #endregion

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
            Script = target as APEvents;

            if (!AssetsLoaded()) return;

            Settings = Script.Settings;

            InitializeSerializedProperties();
        }

        private void OnSceneGUI() {
            if (!AssetsLoaded()) return;

            HandleDrawingMethodNames();
        }

        #endregion

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
                    "APAnimator",
                    "APAnimator component reference."));
        }

        private void DrawInfoLabel(string text) {
            EditorGUILayout.HelpBox(text, MessageType.Error);
        }

        private void DrawReorderableEventList() {

            serializedObject.Update();

            ReorderableListGUI.Title("Events");
            ReorderableListGUI.ListField(nodeEvents);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSettingsAssetField() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                settings,
                new GUIContent(
                    "Settings Asset",
                    "Reference to asset with all APEvents component settings."));

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSkinAssetField() {

            serializedObject.Update();
            EditorGUILayout.PropertyField(skin);
            serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region METHODS
        private bool AssetsLoaded() {
            return (bool)Utilities.InvokeMethodWithReflection(
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
                (Vector3[]) Utilities.InvokeMethodWithReflection(
                    Script,
                    "GetNodePositions",
                    new object[] {methodNames.Length});

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

        #endregion
    }

}