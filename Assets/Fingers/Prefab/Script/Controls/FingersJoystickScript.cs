//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// http://www.digitalruby.com
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    public class FingersJoystickScript : MonoBehaviour
    {
        [Header("Joystick properties")]
        [Tooltip("The image to move around like a joystick")]
        public Image JoystickImage;

        [Tooltip("Reduces the amount the joystick moves the closer it is to the center. As the joystick moves to it's max extents, the movement amount approaches 1. " +
            "For example, a power of 1 would be a linear equation, 2 would be squared, 3 cubed, etc.")]
        [Range(0.01f, 10.0f)]
        public float JoystickPower = 2.0f;

        [Tooltip("The max exten the joystick can move as a percentage of Screen.width + Screen.height")]
        [Range(0.001f, 0.2f)]
        public float MaxExtentPercent = 0.02f;

        [Tooltip("In eight axis mode, the joystick can only move up, down, left, right or diagonally. No in between.")]
        public bool EightAxisMode;

        [Tooltip("Whether the x and y absolute values should add up to 1. If false, both can be 1 at a full diagonal.")]
        public bool AddUpToOne = true;

        [Tooltip("Horizontal input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputHorizontalAxisName;

        [Tooltip("Vertical input axis name if cross platform input integration is desired.")]
        public string CrossPlatformInputVerticalAxisName;

        [Header("Callbacks")]
        public GestureRecognizerComponentEventVector2 JoystickCallback;

        private object crossPlatformInputHorizontalAxisObject;
        private object crossPlatformInputVerticalAxisObject;
        private bool crossPlatformInputNewlyRegistered;

        private Vector2 startCenter;

        private void OnEnable()
        {
            PanGesture = new PanGestureRecognizer
            {
                PlatformSpecificView = (MoveJoystickToGestureStartLocation ? null : JoystickImage.gameObject),
                ThresholdUnits = 0.0f
            };
            PanGesture.AllowSimultaneousExecutionWithAllGestures();
            PanGesture.StateUpdated += PanGestureUpdated;

#if UNITY_EDITOR

            if (JoystickImage == null || JoystickImage.canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                Debug.LogError("Fingers joystick script requires that JoystickImage be set and that the Canvas be in ScreenSpaceOverlay mode.");
            }

#endif

            if (!string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                crossPlatformInputHorizontalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputHorizontalAxisName, out crossPlatformInputNewlyRegistered);
                crossPlatformInputVerticalAxisObject = FingersCrossPlatformInputReflectionScript.RegisterVirtualAxis(CrossPlatformInputVerticalAxisName, out crossPlatformInputNewlyRegistered);
            }

            FingersScript.Instance.AddGesture(PanGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
            }
        }

        private void SetImagePosition(Vector2 pos)
        {
            JoystickImage.rectTransform.anchoredPosition = pos;
        }

        private Vector2 UpdateForEightAxisMode(Vector2 amount, float maxOffset)
        {
            if (EightAxisMode)
            {
                float absX = Mathf.Abs(amount.x);
                float absY = Mathf.Abs(amount.y);
                if (absX > absY * 1.5f)
                {
                    amount.y = 0.0f;
                    amount.x = Mathf.Sign(amount.x) * maxOffset;
                }
                else if (absY > absX * 1.5)
                {
                    amount.x = 0.0f;
                    amount.y = Mathf.Sign(amount.y) * maxOffset;
                }
                else
                {
                    amount.x = Mathf.Sign(amount.x) * maxOffset * 0.7f;
                    amount.y = Mathf.Sign(amount.y) * maxOffset * 0.7f;
                }
            }
            return amount;
        }

        private void ExecuteCallback(Vector2 amount)
        {
            if (JoystickExecuted != null)
            {
                JoystickExecuted(this, amount);
            }
            if (JoystickCallback != null)
            {
                JoystickCallback.Invoke(amount);
            }
        }

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Executing)
            {
                // clamp joystick movement to max values
                float maxOffset = (Screen.width + Screen.height) * MaxExtentPercent;
                Vector2 offset = new Vector2(gesture.FocusX - gesture.StartFocusX, gesture.FocusY - gesture.StartFocusY);

                // check distance from center, clamp to distance
                offset = Vector2.ClampMagnitude(offset, maxOffset);

                // don't bother if no motion
                if (offset == Vector2.zero)
                {
                    return;
                }

                // handle eight axis offset param
                offset = UpdateForEightAxisMode(offset, maxOffset);

                // move image
                SetImagePosition(startCenter + offset);

                if (!AddUpToOne)
                {
                    float maxOffsetLerp = maxOffset * 0.7f;
                    offset.x = Mathf.Sign(offset.x) * Mathf.Lerp(0.0f, maxOffset, Mathf.Abs(offset.x / maxOffsetLerp));
                    offset.y = Mathf.Sign(offset.y) * Mathf.Lerp(0.0f, maxOffset, Mathf.Abs(offset.y / maxOffsetLerp));
                }

                // callback with movement weight
                if (JoystickPower >= 1.0f)
                {
                    // power is reducing offset, apply as is
                    offset.x = Mathf.Sign(offset.x) * Mathf.Pow(Mathf.Abs(offset.x) / maxOffset, JoystickPower);
                    offset.y = Mathf.Sign(offset.y) * Mathf.Pow(Mathf.Abs(offset.y) / maxOffset, JoystickPower);
                }
                else
                {
                    // power is increasing offset, we need to make sure we maintain the aspect ratio of offset
                    Vector2 absOffset = new Vector2(Mathf.Abs(offset.x), Mathf.Abs(offset.y));
                    float offsetTotal = absOffset.x + absOffset.y;
                    float xWeight = absOffset.x / offsetTotal;
                    float yWeight = absOffset.y / offsetTotal;
                    offset.x = xWeight * Mathf.Sign(offset.x) * Mathf.Pow(absOffset.x / maxOffset, JoystickPower);
                    offset.y = yWeight * Mathf.Sign(offset.y) * Mathf.Pow(absOffset.y / maxOffset, JoystickPower);
                    offset = Vector2.ClampMagnitude(offset, maxOffset);
                }
                ExecuteCallback(offset);

                if (crossPlatformInputHorizontalAxisObject!= null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputHorizontalAxisObject, offset.x);
                }
                if (crossPlatformInputVerticalAxisObject != null)
                {
                    FingersCrossPlatformInputReflectionScript.UpdateVirtualAxis(crossPlatformInputVerticalAxisObject, offset.y);
                }
            }
            else if (gesture.State == GestureRecognizerState.Began)
            {
                if (MoveJoystickToGestureStartLocation)
                {
                    JoystickImage.transform.parent.position = new Vector3(gesture.FocusX, gesture.FocusY, JoystickImage.transform.parent.position.z);
                }
                startCenter = JoystickImage.rectTransform.anchoredPosition;
            }
            else if (gesture.State == GestureRecognizerState.Ended)
            {
                // return to center
                SetImagePosition(startCenter);

                // final callback
                ExecuteCallback(Vector2.zero);
            }
        }

        private void Update()
        {

        }

        private void OnDestroy()
        {
            if (crossPlatformInputNewlyRegistered && !string.IsNullOrEmpty(CrossPlatformInputHorizontalAxisName) && !string.IsNullOrEmpty(CrossPlatformInputVerticalAxisName))
            {
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputHorizontalAxisName);
                FingersCrossPlatformInputReflectionScript.UnRegisterVirtualAxis(CrossPlatformInputVerticalAxisName);
            }
        }

        public PanGestureRecognizer PanGesture { get; private set; }
        public System.Action<FingersJoystickScript, Vector2> JoystickExecuted;

        /// <summary>
        /// Whether to move the joystick when the pan gesture starts. Set this in Awake.
        /// </summary>
        public bool MoveJoystickToGestureStartLocation { get; set; }
    }
}
