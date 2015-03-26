using System.Collections.Generic;
using ATP.AnimationPathTools.AnimatorComponent;
using UnityEngine;
// todo use qualified name instead.
using Animator = ATP.AnimationPathTools.AnimatorComponent.Animator;

namespace ATP.AnimationPathTools.EventsComponent {

    [RequireComponent(typeof (Animator))]
    public class Events : MonoBehaviour, ISerializationCallbackReceiver {
        #region FIELDS

        [SerializeField]
        private bool advancedSettingsFoldout;

        [SerializeField]
        private Animator animator;

#pragma warning disable 0414 
        [SerializeField]
        private bool drawMethodNames = true;
#pragma warning restore 0414 

        [SerializeField]
        private List<NodeEventSlot> nodeEventSlots;

        [SerializeField]
        private EventsSettings settings;

        [SerializeField]
        private GUISkin skin;

        #endregion

        #region PROPERTIES

        public Animator Animator {
            get { return animator; }
            set { animator = value; }
        }

        public EventsSettings Settings {
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

            Animator.NodeReached -= Animator_NodeReached;

            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded -= PathData_NodeAdded;
                Animator.PathData.NodeRemoved -= PathData_NodeRemoved;
            }
        }

        private void OnEnable() {
            Debug.Log("OnEnable()");
            if (Animator == null) return;

            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        void PathData_NodeRemoved(object sender, NodeAddedRemovedEventArgs e) {
            NodeEventSlots.RemoveAt(e.NodeIndex);
        }

        void PathData_NodeAdded(object sender, NodeAddedRemovedEventArgs e) {
            NodeEventSlots.Insert(e.NodeIndex, new NodeEventSlot());
        }

        private void Reset() {
            animator = GetComponent<Animator>();

            InitializeSlots();
            LoadRequiredResources();
            UnsubscribeFromEvents();
            SubscribeToEvents();
        }

        private void InitializeSlots() {
            // Instantiate slots list.
            nodeEventSlots = new List<NodeEventSlot>();
            // Get number of nodes in the path.
            var nodesNo = Animator.PathData.NodesNo;

            // For each path node..
            for (int i = 0; i < nodesNo; i++) {
                // Add empty slot.
                NodeEventSlots.Add(new NodeEventSlot());
            }
        }

        private void LoadRequiredResources() {
            settings =
                Resources.Load<EventsSettings>("DefaultEventsSettings");
            skin = Resources.Load("DefaultEventsSkin") as GUISkin;
        }

        // todo add same to other behaviours
        private void OnDestroy() {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents() {
            Animator.NodeReached += Animator_NodeReached;
            Animator.NewPathDataCreated += Animator_NewPathDataCreated;
            Animator.PathDataRefChanged += Animator_PathDataRefChanged;

            if (Animator.PathData != null) {
                Animator.PathData.NodeAdded += PathData_NodeAdded;
                Animator.PathData.NodeRemoved += PathData_NodeRemoved;
            }
        }

        void Animator_PathDataRefChanged(object sender, System.EventArgs e) {
            if (Animator.PathData != null) {
                UnsubscribeFromEvents();
                SubscribeToEvents();
                InitializeSlots();
            }
            else {
                UnsubscribeFromEvents();
            }

        }

        void Animator_NewPathDataCreated(object sender, System.EventArgs e) {

        }


        #endregion

        #region EVENT HANDLERS

        private void Animator_NodeReached(
            object sender,
            NodeReachedEventArgs arg) {

            // Return if there's no event slot created for current path node.
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

        private Vector3[] GetNodePositions(int nodesNo) {
            var nodePositions =
                Animator.GetGlobalNodePositions(nodesNo);

            return nodePositions;
        }

        #endregion

        // todo move to region
        public void OnBeforeSerialize() {
        }

        public void OnAfterDeserialize() {
            SubscribeToEvents();
        }

    }

}