using System.IO;
using UnityEditor;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathAnimatorComponent {

    public sealed class GizmoIcons {

        private PathAnimatorSettings Settings { get; set; }

        public GizmoIcons(PathAnimatorSettings settings) {
            Settings = settings;
        }

        public void CopyIconsToGizmosFolder() {
            var gizmosDir = Application.dataPath + "/Gizmos/";

            // Create Asset/Gizmos folder if not exists.
            if (!Directory.Exists(gizmosDir)) {
                Directory.CreateDirectory(gizmosDir);
            }

            // Check if settings asset has any paths to be searched for icons.
            if (Settings.IconsSourceDirs == null) return;

            // For each path..
            foreach (var iconDir in Settings.IconsSourceDirs) {
                // If source directory exists..
                if (Directory.Exists(Application.dataPath + iconDir)) {
                    var destFilePath =
                        gizmosDir + Settings.CurrentRotationPointGizmoIcon;

                    // If file doesn't exist..
                    if (!File.Exists(destFilePath)) {
                        // Copy icon to Asset/Gizmos folder.
                        CopyIcon(iconDir, Settings.CurrentRotationPointGizmoIcon);
                    }

                    destFilePath =
                        gizmosDir + Settings.ForwardPointIcon;

                    // If file doesn't exist..
                    if (!File.Exists(destFilePath)) {
                        // Copy icon to Asset/Gizmos folder.
                        CopyIcon(iconDir, Settings.ForwardPointIcon);
                    }

                    destFilePath =
                        gizmosDir + Settings.RotationPointGizmoIcon;

                    // If file doesn't exist..
                    if (!File.Exists(destFilePath)) {
                        // Copy icon to Asset/Gizmos folder.
                        CopyIcon(iconDir, Settings.RotationPointGizmoIcon);
                    }

                    destFilePath =
                        gizmosDir + Settings.TargetGizmoIcon;

                    // If file doesn't exist..
                    if (!File.Exists(destFilePath)) {
                        // Copy icon to Asset/Gizmos folder.
                        CopyIcon(iconDir, Settings.TargetGizmoIcon);
                    }

                    break;
                }
            }
        }

        private void CopyIcon(string sourceDir, string iconName) {
            // Check if icon file exists in Assets/Gizmos folder.
            if (!File.Exists(
                Application.dataPath + "/Gizmos/"
                + iconName + ".png")) {

                // TODO If source directory doesn't exist, show info about how
                // to copy icons to the Gizmos folder.

                // Copy icon.
                FileUtil.CopyFileOrDirectory(
                    Application.dataPath
                    + sourceDir
                    + iconName + ".png",
                    Application.dataPath + "/Gizmos/"
                    + iconName + ".png");
            }
        }

    }

}