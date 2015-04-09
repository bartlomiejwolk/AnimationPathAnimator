using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using ATP.LoggingTools;
using UnityEngine;
// todo use qualified name instead.

namespace ATP.AnimationPathTools.AnimatorEventsComponent {

    [RequireComponent(typeof (AnimationPathAnimator))]
    public class AnimatorEvents : MonoBehaviour, ISerializationCallbackReceiver {
        #region FIELDS

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private AnimationPathAnimator animator;

#pragma warning disable 0414 
        [SerializeField]
        private bool drawMethodNames = true;
#pragma warning restore 0414 

        [SerializeField]
        private List<NodeEventSlot> nodeEventSlots;

        [SerializeField]
        private AnimatorEventsSettings settings;

        [SerializeField]
        private GUISkin skin;

        #endregion

        #region PROPERTIES

        public AnimationPathAnimator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public AnimatorEventsSettings Settings {
            get { return settings; }
        }

        public GUISkin Skin {
            get { return skin; }
            set { skin = value; }
        }

        private List<NodeEventSlot> NodeEventSlots {
            get { return nodeEventSlots; }
        }

        #endregion

        #region UNITY MESSAGES

        private void OnDisable() {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents() {
            // Guard agains null reference.
            if (Animator == null) Animator = GetComponent<AnimationPathAnimator>();

            Animator.NodeReached -= Animator_NodeReached;
            Animator.PathDataRefChanged -= Animator_PathDataRefChanged;
            Animator.UndoRedoPerformed -= Animator_UndoRedoPerformed;
        }

        private void OnEnable() {
            Debug.Log("OnEnable()");
            if (Animator == null) return;

            UnsubscribeFromEvents();
            SubscribeToAnimatorEvents();
        }

        private void OnValidate() {
            Logger.LogCall();
            UnsubscribeFromEvents();
            SubscribeToAnimatorEvents();
        }

        void PathData_NodeRemoved(object sender, NodeAddedRemovedEventArgs e) {
            NodeEventSlots.RemoveAt(e.NodeIndex);
        }

        void PathData_NodeAdded(object sender, NodeAddedRemovedEventArgs e) {
            Debug.Log("NodeAdded event");
            NodeEventSlots.Insert(e.NodeIndex, new NodeEventSlot());
        }

        private void Reset() {
            Animator = GetComponent<AnimationPathAnimator>();
            nodeEventSlots = new List<NodeEventSlot>();

            InitializeSlots();
            LoadRequiredResources();
            UnsubscribeFromEvents();
            SubscribeToAnimatorEvents();
        }

        private void InitializeSlots() {
            // Get number of nodes in the path.
            var nodesNo = Animator.PathData.NodesNo;

            // Calculate how many slots to add/remove.
            var slotsDiff = NodeEventSlots.Count - nodesNo;

            if (slotsDiff > 0) {
                // Remove slots.
                for (int i = 0; i < slotsDiff; i++) {
                    NodeEventSlots.RemoveAt(NodeEventSlots.Count - 1);
                }
            }
            else {
                // Add slots
                for (int i = 0; i < Mathf.Abs(slotsDiff); i++) {
                    NodeEventSlots.Add(new NodeEventSlot());
                }
            }

            Utilities.Assert(
                () => nodesNo == NodeEventSlots.Count,
                string.Format("Number of nodes ({0}) in the path and event slots ({1}) differ.",
                nodesNo,
                NodeEventSlots.Count));
        }

        private void LoadRequiredResources() {
            settings =
                Resources.Load<AnimatorEventsSettings>("DefaultAnimatorEventsSettings");
            skin = Resources.Load("DefaultAnimatorEventsSkin") as GUISkin;
        }

        // todo add same to other behaviours
        private void OnDestroy() {
            UnsubscribeFromEvents();
        }

        private void SubscribeToAnimatorEvents() {
            // Guard agains null reference.
            if (Animator == null) Animator = GetComponent<AnimationPathAnimator>();

            Animator.NodeReached += Animator_NodeReached;
            Animator.PathDataRefChanged += Animator_PathDataRefChanged;
            Animator.UndoRedoPerformed += Animator_UndoRedoPerformed;
        }


        void Animator_UndoRedoPerformed(object sender, System.EventArgs e) {
            // During animator undo event, reference to path data could have been changed.
            //HandlePathDataRefChange();
        }

        void Animator_PathDataRefChanged(object sender, System.EventArgs e) {
            HandlePathDataRefChange();
        }

        private void HandlePathDataRefChange() {
            // todo invert condition (if == null)
            if (Animator.PathData != null) {
                UnsubscribeFromEvents();
                //SubscribeToAnimatorEvents();
                SubscribeToPathEvents();
                InitializeSlots();
            }
            else {
                //UnsubscribeFromEvents();
                UnsubscribeFromPathEvents();
                //NodeEventSlots.Clear();
            }
        }

        private void SubscribeToPathEvents() {
            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded += PathData_NodeAdded;
                Animator.PathData.NodeRemoved += PathData_NodeRemoved;
            }
        }

        private void UnsubscribeFromPathEvents() {
            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded -= PathData_NodeAdded;
                Animator.PathData.NodeRemoved -= PathData_NodeRemoved;
            }
        }

        #endregion

        #region EVENT HANDLERS

        private void Animator_NodeReached(
            object sender,
            NodeReachedEventArgs arg) {

            // Return if there's no event slot created for current path node.
            // todo remove. Now every node has its own slot.
            if (arg.NodeIndex > NodeEventSlots.Count - 1) return;
            // Get event slot.
            var nodeEvent = NodeEventSlots[arg.NodeIndex];
            // Return if source GO was not specified in the event slot.
            if (nodeEvent.SourceGO == null) return;
            // Get method metadata.
            var methodInfo = nodeEvent.SourceCo.GetType()
                .GetMethod(nodeEvent.SourceMethodName);
            // Return if method info couldn't be loaded.
            if (methodInfo == null) return;
            // Get method parameters.
            var methodParams = methodInfo.GetParameters();

            // Method has no parameters.
            if (methodParams.Length == 0) {
                // Invoke method.
                methodInfo.Invoke(nodeEvent.SourceCo, null);
            }
            // Method has one parameter.
            else if (methodParams.Length == 1) {
                // Return if the parameter is not a string.
                if (methodParams[0].ParameterType.Name != "String") return;
                // Create string parameter argument.
                var stringParam = new object[] { nodeEvent.MethodArg };
                // Invoke method with string parameter.
                methodInfo.Invoke(nodeEvent.SourceCo, stringParam);
            }
        }

        #endregion

        #region METHODS

        private bool RequiredAssetsLoaded() {
            if (Settings != null
                && Skin != null) {
                return true;
            }
            return false;
        }

        private string[] GetMethodNames() {
            // todo this may not be necessary if slots are always in sync with nodes
            // Return empty array is slots list was not yet initalized.
            if (NodeEventSlots == null) return new string[0];

            var methodNames = new string[NodeEventSlots.Count];

            for (var i = 0; i < NodeEventSlots.Count; i++) {
                methodNames[i] = NodeEventSlots[i].SourceMethodName;
            }

            return methodNames;
        }

        private List<Vector3> GetNodePositions(int nodesNo) {
            var nodePositions =
                Animator.GetGlobalNodePositions(nodesNo);

            return nodePositions;
        }

        #endregion

        // todo move to region
        public void OnBeforeSerialize() {
        }

        public void OnAfterDeserialize() {
        }

    }

}