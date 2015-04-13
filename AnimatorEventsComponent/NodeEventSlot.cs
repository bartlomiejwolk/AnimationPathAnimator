/* 
 * Copyright (c) 2015 Bart³omiej Wo³k (bartlomiejwolk@gmail.com).
 *
 * This file is part of the AnimationPath Animator Unity extension.
 * Licensed under the MIT license. See LICENSE file in the project root folder.
 */

using System;
using UnityEngine;

namespace ATP.AnimationPathTools.AnimatorEventsComponent {

    [Serializable]
    public sealed class NodeEventSlot {

        [SerializeField]
        private string methodArg;

#pragma warning disable 0414
        /// <summary>
        ///     How many rows should be displayed in the inspector.
        /// </summary>
        [SerializeField]
        private int rows = 1;
#pragma warning restore 0414

        /// <summary>
        ///     Selected source component.
        /// </summary>
        [SerializeField]
        private Component sourceCo;

        [SerializeField]
        private int sourceComponentIndex;

        /// <summary>
        ///     Selected source game object.
        /// </summary>
        [SerializeField]
        private GameObject sourceGO;

        [SerializeField]
        private int sourceMethodIndex;

        [SerializeField]
        private string sourceMethodName;

        public string MethodArg {
            get { return methodArg; }
            set { methodArg = value; }
        }

        /// <summary>
        ///     Selected source component.
        /// </summary>
        public Component SourceCo {
            get { return sourceCo; }
        }

        /// <summary>
        ///     Selected source game object.
        /// </summary>
        public GameObject SourceGO {
            get { return sourceGO; }
        }

        public string SourceMethodName {
            get { return sourceMethodName; }
        }

    }

}