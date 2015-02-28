using System;
using UnityEngine;

namespace ATP.AnimationPathTools {

    public static class ShortcutHandler {

        // TODO Replace all with properties.
        public const KeyCode MoveAllKey = KeyCode.U;
        public const KeyCode MoveSingleModeKey = KeyCode.Y;
        public const KeyCode NoneModeShortcut = KeyCode.K;
        public const KeyCode PlayPauseShortcut = KeyCode.Space;
        public const KeyCode RotationModeShortcut = KeyCode.H;
        public const KeyCode TiltingModeShortcut = KeyCode.J;
        public const KeyCode UpdateAllShortcut = KeyCode.L;

        public const KeyCode EaseModeShortcut = KeyCode.G;

        /// <summary>
        ///     Key shortcut to jump backward.
        /// </summary>
        public const KeyCode JumpBackward = KeyCode.LeftArrow;

        /// <summary>
        ///     Key shortcut to jump forward.
        /// </summary>
        public const KeyCode JumpForward = KeyCode.RightArrow;

        /// <summary>
        ///     Key shortcut to jump to the end of the animation.
        /// </summary>
        public const KeyCode JumpToEnd = KeyCode.UpArrow;

        public const KeyCode JumpToNextNode = KeyCode.UpArrow;
        public const KeyCode JumpToPreviousNode = KeyCode.DownArrow;

        /// <summary>
        ///     Key shortcut to jump to the beginning of the animation.
        /// </summary>
        public const KeyCode JumpToStart = KeyCode.DownArrow;

        /// <summary>
        ///     Keycode used as a modifier key.
        /// </summary>
        /// <remarks>Modifier key changes how other keys works.</remarks>
        public const KeyCode ModKey = KeyCode.A;

    }

}