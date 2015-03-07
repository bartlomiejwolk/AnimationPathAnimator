using System;
using UnityEngine;

namespace ATP.SimplePathAnimator.PathAnimatorComponent {
    public class GameComponent : MonoBehaviour {

        public enum InfoType { Warning, Error }

        public void MissingReferenceError(
            string fieldName,
            string detailedInfo = "") {
            MissingReference(fieldName, InfoType.Error, detailedInfo);
        }

        public void MissingReferenceWarning(
            string fieldName,
            string detailedInfo = "") {
            MissingReference(fieldName, InfoType.Error, detailedInfo);
        }

        /// Log info about missing reference.
        ///
        /// \param fieldName name of the missing reference field.
        /// \param type type of the info message.
        private void MissingReference(
            string fieldName,
            InfoType type,
            string detailedInfo) {

            // Message to be displayed.
            string message;

            message =
                "Component reference is missing in: "
                + transform.root
                + " / "
                + gameObject.name
                + " '"
                + this.GetType()
                + "'"
                + " / "
                + fieldName
                + " : "
                + detailedInfo;

            switch (type) {
                case InfoType.Warning:
                    Debug.LogWarning(message, transform);
                    break;
                case InfoType.Error:
                    Debug.LogError(message, transform);
                    break;
            }
        }
    }
}

