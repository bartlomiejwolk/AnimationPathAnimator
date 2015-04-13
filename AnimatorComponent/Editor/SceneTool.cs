/* 
 * Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using UnityEditor;

namespace AnimationPathTools.AnimatorComponent {

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