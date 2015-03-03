using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator {

    public static class PathExporter {

        public static void DrawExportControls(AnimationPathAnimator script) {
            EditorGUILayout.BeginHorizontal();

            script.ExportSamplingFrequency = EditorGUILayout.IntField(
                new GUIContent(
                    "Export Sampling",
                    "Number of points to export for 1 m of the curve. " +
                    "If set to 0, it'll export only keys defined in " +
                    "the curve."),
                script.ExportSamplingFrequency);

            if (GUILayout.Button("Export")) {
                ExportNodes(
                    script.PathData,
                    script.Transform,
                    script.ExportSamplingFrequency);
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
            Transform transform,
            int exportSampling) {

            // exportSampling cannot be less than 0.
            if (exportSampling < 0) return;

            // Points to be exported.
            List<Vector3> points;

            // Initialize points array with nodes to export.
            points = pathData.SampleAnimationPathForPoints(
                exportSampling);

            // Convert points to global coordinates.
            for (int i = 0; i < points.Count; i++) {
                points[i] = transform.TransformPoint(points[i]);
            }

            // Create parent GO.
            var exportedPath = new GameObject("exported_path");

            // Create child GOs.
            for (var i = 0; i < points.Count; i++) {
                // Create child GO.
                var nodeGo = new GameObject("Node " + i);

                // Move node under the path GO.
                nodeGo.transform.parent = exportedPath.transform;

                // Assign node local position.
                nodeGo.transform.localPosition = points[i];
            }
        }

    }

}