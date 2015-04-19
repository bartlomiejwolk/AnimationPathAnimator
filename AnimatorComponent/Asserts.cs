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
        public static void AssertNodeToolInSync(
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

    }

}
