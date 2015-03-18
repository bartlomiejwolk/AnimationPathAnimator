using UnityEditor;

namespace ATP.AnimationPathAnimator.APAnimatorComponent {

    public static class SceneTool {

        private static Tool lastTool = Tool.None;

        public static Tool LastTool {
            get { return lastTool; }
            set { lastTool = value; }
        }

        public static void RememberCurrentTool() {
            // Remember active scene tool.
            if (Tools.current == Tool.None) return;

            LastTool = Tools.current;
            Tools.current = Tool.None;
        }

        public static void RestoreTool() {
            Tools.current = LastTool;
        }

    }

}