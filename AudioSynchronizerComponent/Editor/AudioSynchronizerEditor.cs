// Copyright (c) 2015 Bartłomiej Wołk (bartlomiejwolk@gmail.com)
//  
// This file is part of the AnimationPath Animator extension for Unity.
// Licensed under the MIT license. See LICENSE file in the project root folder.

using UnityEditor;
using UnityEngine;

namespace AnimationPathAnimator.AudioSynchronizerComponent {

    [CustomEditor(typeof (AudioSynchronizer))]
    public sealed class AudioSynchronizerEditor : Editor {

        private SerializedProperty animator;
        private SerializedProperty audioSource;
        private SerializedProperty autoPlay;
        private SerializedProperty autoPlayDelay;

        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.PropertyField(
                audioSource,
                new GUIContent(
                    "Audio Source",
                    "AudioSource component reference."));

            EditorGUILayout.PropertyField(
                animator,
                new GUIContent(
                    "Animator",
                    "Animator component reference."));

            EditorGUILayout.Space();

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.PropertyField(
                autoPlay,
                new GUIContent(
                    "Auto Play",
                    "Play on enter game mode."));

            EditorGUIUtility.labelWidth = 50;

            EditorGUILayout.PropertyField(
                autoPlayDelay,
                new GUIContent(
                    "Delay",
                    "Auto play delay in seconds."));

            EditorGUIUtility.labelWidth = 00;

            EditorGUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable() {
            audioSource = serializedObject.FindProperty("audioSource");
            animator = serializedObject.FindProperty("animator");
            autoPlay = serializedObject.FindProperty("autoPlay");
            autoPlayDelay = serializedObject.FindProperty("autoPlayDelay");
        }

    }

}