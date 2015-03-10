using System.IO;
using UnityEditor;
using UnityEngine;

namespace ATP.AnimationPathAnimator.AnimatorComponent {

    public sealed class GizmoIcons {

        private PathAnimatorSettings Settings { get; set; }

        public GizmoIcons(PathAnimatorSettings settings) {
            Settings = settings;
        }

        public void CopyIconsToGizmosFolder() {
            // Path to Unity Gizmos folder.
            var gizmosDir = Application.dataPath + "/Gizmos";

            // Path to ATP folder inside Gizmos.

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(gizmosDir + "ATP")) {
                Directory.CreateDirectory(gizmosDir + "ATP");
            }

            // Check if settings asset has icons specified.
            if (Settings.GizmoIcons == null) return;

            // For each icon..
            foreach (var icon in Settings.GizmoIcons) {
                // Get icon path.
                var iconPath = AssetDatabase.GetAssetPath(icon);

                // Copy icon to Gizmos folder.
                AssetDatabase.CopyAsset(iconPath, gizmosDir + "/ATP");
            }
        }
    }

}