// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
//  
// This file is part of the AnimationPath Animator Unity extension.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using AnimationPathTools.AnimatorComponent;
using AnimationPathTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace AnimationPathTools.AnimatorSynchronizerComponent {

    [CustomEditor(typeof (AnimatorSynchronizer))]
    public class AnimatorSynchronizerEditor : Editor {

        private AnimatorSynchronizer Script;
        private SerializedProperty targetComponents;

        public override void OnInspectorGUI() {
            DrawSourceAnimatorField();
            DrawTargetAnimatorComponentList();
        }

        private void DrawSourceAnimatorField() {
            Script.Animator =
                (PathAnimator) EditorGUILayout.ObjectField(
                    new GUIContent(
                        "Animator",
                        ""),
                    Script.Animator,
                    typeof (PathAnimator),
                    true);
        }

        private void DrawTargetAnimatorComponentList() {
            serializedObject.Update();

            ReorderableListGUI.Title("Target Animator Components");
            ReorderableListGUI.ListField(targetComponents);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            Script = (AnimatorSynchronizer) target;

            targetComponents = serializedObject.FindProperty("targetComponents");
        }

    }

}