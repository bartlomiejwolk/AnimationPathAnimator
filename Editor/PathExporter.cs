using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public static class PathExporter {

        public static void DrawExportControls(
            SerializedObject serializedObject,
            SerializedProperty exportSamplingFrequency,
            PathData pathData) {

            EditorGUILayout.BeginHorizontal();
            serializedObject.Update();
            EditorGUILayout.PropertyField(exportSamplingFrequency,
                new GUIContent(
                    "Export Sampling",
                    "Number of points to export for 1 m of the curve. " +
                    "If set to 0, it'll export only keys defined in " +
                    "the curve."));
            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("Export")) {
                ExportNodes(
                    pathData,
                    exportSamplingFrequency.intValue);
            }
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Export Animation Path nodes as transforms.
        /// </summary>
        /// <param name="exportSampling">
        ///     Amount of result transforms for one meter of Animation Path.
        /// </param>
        private static void ExportNodes(
            PathData pathData,
            int exportSampling) {

            // Points to be exported.
            List<Vector3> points;

            // If exportSampling arg. is zero then export one transform for
            // each Animation Path node.
            if (exportSampling == 0) {
                // Initialize points.
                points = new List<Vector3>(pathData.NodesNo);

                // For each node in the path..
                for (var i = 0; i < pathData.NodesNo; i++) {
                    // Get it 3d position.
                    points[i] = pathData.GetNodePosition(i);
                }
            }
            // exportSampling not zero..
            else {
                // Initialize points array with nodes to export.
                points = pathData.SampleAnimationPathForPoints(
                    exportSampling);
            }

            // Create parent GO.
            var exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (var i = 0; i < points.Count; i++) {
                // Create child GO.
                var node = new GameObject("Node " + i);

                // Move node under the path GO.
                node.transform.parent = exportedPath.transform;

                // Assign node local position.
                node.transform.localPosition = points[i];
            }
        }

    }

}