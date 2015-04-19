using System.Collections;

namespace AnimationPathAnimator.AnimatorComponent {

    public static class Asserts {

        /// <summary>
        /// Assert that number of nodes in the path and the number of entries in
        /// the array that allows enabling node tools is the same.
        /// </summary>
        /// <param name="nodesNo">Number of nodes in the path.</param>
        /// <param name="toolStateNo">Number of entries in the tool state list.</param>
        /// <param name="toolName">Name of the tool ("ease" or "tilting").</param>
        public static void AssertEnabledToolsListInSync(
            int nodesNo,
            int toolStateNo,
            string toolName) {

            Utilities.Assert(
                () => nodesNo == toolStateNo,
                string.Format(
                    "Number of nodes in the path ({0}) is " +
                    "different from number of entries in the " +
                    "{2} tool ({1}).",
                    nodesNo,
                    toolStateNo,
                    toolName));
        }

        public static void AssertToolCurveInSync(
            int nodesWithToolEnabled,
            int toolCurveKeysNo,
            string toolName) {

            Utilities.Assert(
                () => nodesWithToolEnabled
                      == toolCurveKeysNo,
                      string.Format("Number of path nodes ({0}) with enabled {2} tool is different"
                                    + " from number of {2} curve keys ({1}).",
                                    nodesWithToolEnabled,
                                    toolCurveKeysNo,
                                    toolName));

        }

    }

}
