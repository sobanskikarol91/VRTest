using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_2018_4_OR_NEWER
using UnityEngine.XR;
#endif
#if STEAM_VR_SDK
using Valve.VR;
#endif


namespace BNG {

    public enum ControllerHand {
        Left,
        Right,
        None
    }

    public enum HandControl {
        LeftGrip,
        RightGrip,
        LeftTrigger,
        RightTrigger,
        None
    }

    public enum GrabButton {
        Grip,
        Trigger
    }

    public enum XRInputSource {
        XRInput,
        OVRInput,
        SteamVR
    }    

    /// <summary>
    /// A proxy for handling input from various input providers such as OVRInput, XRInput, and SteamVR. 
    /// </summary>
    public class InputBridge : MonoBehaviour {

        /// <summary>
        /// Instance of our Singleton
        /// </summary>
        public static InputBridge Instance {
            get {
                if (_instance == null) {
                    _instance = FindObjectOfType<InputBridge>();
                    if (_instance == null) {
                        _instance = new GameObject("InputBridge").AddComponent<InputBridge>();
                    }
                }
                return _instance;
            }
        }
        private static InputBridge _instance;

        [SerializeField]
        //private XRInputSource inputSource;
        public XRInputSource InputSource = XRInputSource.OVRInput;

        /// <summary>
        /// How far Left Grip is Held down. Values : 0 - 1 (Fully Open / Closed)
        /// </summary>
        public float LeftGrip = 0;

        /// <summary>
        /// Left Grip was pressed down this drame, but not last
        /// </summary>
        public bool LeftGripDown = false;

        /// <summary>
        /// How far Right Grip is Held down. Values : 0 - 1 (Fully Open / Closed)
        /// </summary>
        public float RightGrip = 0;

        /// <summary>
        /// Right Grip was pressed down this drame, but not last
        /// </summary>
        public bool RightGripDown = false;

        public float LeftTrigger = 0;
        public bool LeftTriggerNear = false;
        public bool LeftTriggerDown = false;
        public float RightTrigger = 0;
        public bool RightTriggerDown = false;
        public bool RightTriggerNear = false;

        public bool LeftThumbNear = false;
        public bool RightThumbNear = false;

        /// <summary>
        /// Pressed down this drame, but not last
        /// </summary>
        public bool LeftThumbstickDown = false;
        public bool RightThumbstickDown = false;

        /// <summary>
        /// CurrentlyHeldDown
        /// </summary>
        public bool LeftThumbstick = false;
        public bool RightThumbstick = false;

        // Oculus Touch Controllers
        public bool AButton = false;

        // A Button Down this frame but not last
        public bool AButtonDown = false;

        public bool BButton = false;

        // A Button Up this frame but down the last
        public bool AButtonUp = false;

        // B Button Down this frame but not last
        public bool BButtonDown = false;

        // B Button Up this frame but down the last
        public bool BButtonUp = false;

        public bool XButton = false;
        public bool XButtonDown = false;

        // X Button Up this frame but down the last
        public bool XButtonUp = false;

        public bool YButton = false;
        public bool YButtonDown = false;
        public bool YButtonUp = false;

        public bool StartButton = false;
        public bool StartButtonDown = false;
        public bool BackButton = false;
        public bool BackButtonDown = false;

        public Vector2 LeftThumbstickAxis;
        public Vector2 RightThumbstickAxis;

        /// <summary>
        /// Thumbstick X must be greater than this amount to be considered valid
        /// </summary>
        [Tooltip("Thumbstick X must be greater than this amount to be considered valid")]
        public float ThumbstickDeadzoneX = 0.001f;

        /// <summary>
        /// Thumbstick Y must be greater than this amount to be considered valid
        /// </summary>
        [Tooltip("Thumbstick Y must be greater than this amount to be considered valid")]
        public float ThumbstickDeadzoneY = 0.001f;

#if UNITY_2019_2_OR_NEWER
        static List<InputDevice> devices = new List<InputDevice>();
#endif

        // What threshold constitutes a "down" event.
        // For example, pushing the trigger down 20% (0.2) of the way considered starting a trigger down event
        // This is used in XRInput
        private float _downThreshold = 0.2f;

        bool XRInputSupported = false;
        bool SteamVRSupport = false;

        private void Awake() {
            // Destroy any duplicate instances that may have been created
            if (_instance != null && _instance != this) {
                Destroy(this);
                return;
            }
           
            _instance = this;

            DontDestroyOnLoad(gameObject);
        }

        void Start() {
#if UNITY_2019_3_OR_NEWER
            XRInputSupported = true;
#endif

#if STEAM_VR_SDK
            SteamVRSupport = true;
            SteamVR.Initialize();
#endif
        }

        void Update() {
            UpdateInputs();
        }

        public virtual void UpdateInputs() {

            string name = transform.name;

            // SteamVR uses an action system
            if (InputSource == XRInputSource.SteamVR && SteamVRSupport) {
                UpdateSteamInput();
            }
            // Use OVRInput to get more Oculus Specific inputs, such as "Near Touch"
            else if (InputSource == XRInputSource.OVRInput || !XRInputSupported) {

                LeftThumbstickAxis = ApplyDeadZones(OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick), ThumbstickDeadzoneX, ThumbstickDeadzoneY);
                RightThumbstickAxis = ApplyDeadZones(OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick), ThumbstickDeadzoneX, ThumbstickDeadzoneY);

                LeftGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
                LeftGripDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);

                RightGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
                RightGripDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

                LeftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
                LeftTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

                RightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
                RightTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

                LeftTriggerNear = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
                LeftThumbNear = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, OVRInput.Controller.LTouch);

                RightTriggerNear = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
                RightThumbNear = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, OVRInput.Controller.RTouch);

                SetOVRButtons();
            }
            else {
#if UNITY_2019_3_OR_NEWER
                // Refresh XR devices
                InputDevices.GetDevices(devices);

                // Left XR Controller
                var leftHandedControllers = new List<InputDevice>();
                var dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller;
                InputDevices.GetDevicesWithCharacteristics(dc, leftHandedControllers);
                var primaryLeftController = leftHandedControllers.FirstOrDefault();

                // Right XR Controller
                var rightHandedControllers = new List<InputDevice>();
                dc = InputDeviceCharacteristics.HeldInHand | InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller;
                InputDevices.GetDevicesWithCharacteristics(dc, rightHandedControllers);
                var primaryRightController = rightHandedControllers.FirstOrDefault();

                LeftThumbstickAxis = ApplyDeadZones(getFeatureUsage(primaryLeftController, CommonUsages.primary2DAxis), ThumbstickDeadzoneX, ThumbstickDeadzoneY);
                RightThumbstickAxis = ApplyDeadZones(getFeatureUsage(primaryRightController, CommonUsages.primary2DAxis), ThumbstickDeadzoneX, ThumbstickDeadzoneY);

                // Store copy of previous value so we can determin if we need to call OnDownEvent
                var prevVal = LeftGrip;
                LeftGrip = getFeatureUsage(primaryLeftController, CommonUsages.grip);
                LeftGripDown = prevVal < _downThreshold && LeftGrip >= _downThreshold;

                prevVal = RightGrip;
                RightGrip = getFeatureUsage(primaryRightController, CommonUsages.grip);
                RightGripDown = prevVal < _downThreshold && RightGrip >= _downThreshold;

                prevVal = LeftTrigger;
                LeftTrigger = getFeatureUsage(primaryLeftController, CommonUsages.trigger);
                LeftTriggerDown = prevVal < _downThreshold && LeftTrigger >= _downThreshold;

                prevVal = RightTrigger;
                RightTrigger = getFeatureUsage(primaryRightController, CommonUsages.trigger);
                RightTriggerDown = prevVal < _downThreshold && RightTrigger >= _downThreshold;

                LeftTriggerNear = getFeatureUsage(primaryLeftController, CommonUsages.indexTouch) > 0;
                LeftThumbNear = getFeatureUsage(primaryLeftController, CommonUsages.thumbTouch) > 0;

                RightTriggerNear = getFeatureUsage(primaryRightController, CommonUsages.indexTouch) > 0;
                RightThumbNear = getFeatureUsage(primaryRightController, CommonUsages.thumbTouch) > 0;

                // Let OVRInput Handle the remaining buttons
                SetOVRButtons();
#endif
            }
        }

        public void SetOVRButtons() {
            // OVRInput can typically handle these inputs well :
            AButton = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
            AButtonDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);
            AButtonUp = OVRInput.GetUp(OVRInput.Button.One, OVRInput.Controller.RTouch);

            BButton = OVRInput.Get(OVRInput.Button.Two);
            BButtonDown = OVRInput.GetDown(OVRInput.Button.Two);
            BButtonUp = OVRInput.GetUp(OVRInput.Button.Two);

            XButton = OVRInput.Get(OVRInput.Button.Three);
            XButtonDown = OVRInput.GetDown(OVRInput.Button.Three);
            XButtonUp = OVRInput.GetUp(OVRInput.Button.Three);

            YButton = OVRInput.Get(OVRInput.Button.Four);
            YButtonDown = OVRInput.GetDown(OVRInput.Button.Four);
            YButtonUp = OVRInput.GetUp(OVRInput.Button.Three);

            StartButton = OVRInput.Get(OVRInput.Button.Start);
            StartButtonDown = OVRInput.GetDown(OVRInput.Button.Start);

            BackButton = OVRInput.Get(OVRInput.Button.Back);
            BackButtonDown = OVRInput.GetDown(OVRInput.Button.Back);

            LeftThumbstickDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch);
            RightThumbstickDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch);

            LeftThumbstick = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.LTouch);
            RightThumbstick = OVRInput.Get(OVRInput.Button.PrimaryThumbstick, OVRInput.Controller.RTouch);
        }

        public virtual void UpdateSteamInput() {
#if STEAM_VR_SDK

            LeftThumbstickAxis = ApplyDeadZones(SteamVR_Actions.vRIF_LeftThumbstickAxis.axis, ThumbstickDeadzoneX, ThumbstickDeadzoneY);
            RightThumbstickAxis = ApplyDeadZones(SteamVR_Actions.vRIF_RightThumbstickAxis.axis, ThumbstickDeadzoneX, ThumbstickDeadzoneY);
            LeftThumbstick = SteamVR_Actions.vRIF_LeftThumbstickDown.state;
            LeftThumbstickDown = SteamVR_Actions.vRIF_LeftThumbstickDown.stateDown;
            RightThumbstick = SteamVR_Actions.vRIF_RightThumbstickDown.state;
            RightThumbstickDown = SteamVR_Actions.vRIF_RightThumbstickDown.stateDown;
            LeftThumbNear = SteamVR_Actions.vRIF_LeftThumbstickNear.state;
            RightThumbNear = SteamVR_Actions.vRIF_RightThumbstickNear.state;

            var prevVal = LeftGrip;
            LeftGrip = LeftGrip = correctValue(SteamVR_Actions.vRIF_LeftGrip.axis);
            LeftGripDown = prevVal < _downThreshold && LeftGrip >= _downThreshold;

            prevVal = RightGrip;
            RightGrip = correctValue(SteamVR_Actions.vRIF_RightGrip.axis);
            RightGripDown = prevVal < _downThreshold && RightGrip >= _downThreshold;
            
            LeftTrigger = correctValue(SteamVR_Actions.vRIF_LeftTrigger.axis);
            RightTrigger = correctValue(SteamVR_Actions.vRIF_RightTrigger.axis);
            AButton = SteamVR_Actions.vRIF_AButton.state;
            AButtonDown = SteamVR_Actions.vRIF_AButton.stateDown;
            BButton = SteamVR_Actions.vRIF_BButton.state;
            BButtonDown = SteamVR_Actions.vRIF_BButton.stateDown;
            XButton = SteamVR_Actions.vRIF_XButton.state;
            XButtonDown = SteamVR_Actions.vRIF_XButton.stateDown;
            YButton = SteamVR_Actions.vRIF_YButton.state;
            YButtonDown = SteamVR_Actions.vRIF_YButton.stateDown;
#endif
        }

        float correctValue(float inputValue) {
            return (float)System.Math.Round(inputValue * 1000f) / 1000f;
        }

        Vector2 ApplyDeadZones(Vector2 pos, float deadZoneX, float deadZoneY) {

            // X Positive
            if (pos.x > 0 && pos.x < deadZoneX) {
                pos = new Vector2(deadZoneX, pos.y);
            }
            // X Negative
            else if (pos.x < 0 && pos.x > -ThumbstickDeadzoneX) {
                pos = new Vector2(-deadZoneX, pos.y);
            }

            // Y Positive
            if (pos.y > 0 && pos.y < deadZoneY) {
                pos = new Vector2(pos.y, deadZoneY);
            }
            // Y Negative
            else if (pos.y < 0 && pos.y > -ThumbstickDeadzoneY) {
                pos = new Vector2(pos.y, -deadZoneY);
            }

            return pos;
        }

        public virtual bool IsOculusDevice() {
#if UNITY_2019_2_OR_NEWER
            return XRSettings.loadedDeviceName != null && XRSettings.loadedDeviceName.ToLower().Contains("oculus");
#else
                return true;
#endif
        }

        public virtual bool IsOculusQuest() {
            string model = XRDevice.model;
            if (model == "Oculus Quest" || model == "Quest") {
                return true;
            }

#if UNITY_2019_2_OR_NEWER
            return IsOculusDevice() && XRDevice.refreshRate == 72f;
#else
            if (Application.platform == RuntimePlatform.Android) {
                return true;
            }

            return false;
#endif
        }

        public virtual bool IsHTCDevice() {
#if UNITY_2019_2_OR_NEWER
            return XRSettings.loadedDeviceName.StartsWith("HTC");
#else
                return false;
#endif
        }

#if UNITY_2019_2_OR_NEWER
        float getFeatureUsage(InputDevice device, InputFeatureUsage<float> usage, bool clamp = true) {
            float val;
            device.TryGetFeatureValue(usage, out val);

            return Mathf.Clamp01(val);
        }

        bool getFeatureUsage(InputDevice device, InputFeatureUsage<bool> usage) {
            bool val;
            if (device.TryGetFeatureValue(usage, out val)) {
                return val;
            }

            return val;
        }

        Vector2 getFeatureUsage(InputDevice device, InputFeatureUsage<Vector2> usage) {
            Vector2 val;
            if (device.TryGetFeatureValue(usage, out val)) {
                return val;
            }

            return val;
        }
#endif

#if UNITY_2019_3_OR_NEWER
        public void SetTrackingOrigin(TrackingOriginModeFlags trackingOrigin) {
            // Set to Floor Mode
            List<XRInputSubsystem> subsystems = new List<XRInputSubsystem>();
            SubsystemManager.GetInstances(subsystems);
            for (int i = 0; i < subsystems.Count; i++) {
                subsystems[i].TrySetTrackingOriginMode(trackingOrigin);
            }
        }
#endif

        // Start Vibration on controller
        public void VibrateController(float frequency, float amplitude, float duration, ControllerHand hand) {
            StartCoroutine(Vibrate(frequency, amplitude, duration, hand));
        }

        IEnumerator Vibrate(float frequency, float amplitude, float duration, ControllerHand hand) {
            // Start vibration
            if (hand == ControllerHand.Right) {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
            }
            else if (hand == ControllerHand.Left) {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);
            }

            yield return new WaitForSeconds(duration);

            // Stop vibration
            if (hand == ControllerHand.Right) {
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            }
            else if (hand == ControllerHand.Left) {
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            }
        }
    }
}