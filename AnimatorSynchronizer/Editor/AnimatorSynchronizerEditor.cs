/* 
 * Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using ATP.AnimationPathTools.ReorderableList;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorSynchronizerComponent {

    [CustomEditor(typeof (AnimatorSynchronizer))]
    public class AnimatorSynchronizerEditor : Editor {

        private SerializedProperty targetComponents;

        private AnimatorSynchronizer Script;

        private void OnEnable() {
            Script = (AnimatorSynchronizer) target;

            targetComponents = serializedObject.FindProperty("targetComponents");
        }

        public override void OnInspectorGUI() {
            DrawSourceAnimatorField();
            DrawTargetAnimatorComponentList();
        }

        private void DrawTargetAnimatorComponentList() {
            serializedObject.Update();

            ReorderableListGUI.Title("Target Animator Components");
            ReorderableListGUI.ListField(targetComponents);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSourceAnimatorField() {
            Script.Animator = (AnimatorComponent.AnimationPathAnimator)EditorGUILayout.ObjectField(
                new GUIContent(
                    "Animator",
                    ""),
                Script.Animator,
                typeof (AnimatorComponent.AnimationPathAnimator));
        }

    }

}
