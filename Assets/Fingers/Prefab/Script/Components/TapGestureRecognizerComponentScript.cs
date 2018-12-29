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

namespace DigitalRubyShared
{
    [AddComponentMenu("Fingers Gestures/Gesture/Tap Gesture", 0)]
    public class TapGestureRecognizerComponentScript : GestureRecognizerComponentScript<TapGestureRecognizer>
    {
        [Header("Tap gesture properties")]
        [Tooltip("How many taps must execute in order to end the gesture")]
        [Range(1, 5)]
        public int NumberOfTapsRequired = 1;

        [Tooltip("How many seconds can expire before the tap is released to still count as a tap")]
        [Range(0.01f, 1.0f)]
        public float ThresholdSeconds = 0.5f;

        [Tooltip("How many units away the tap down and up and subsequent taps can be to still be considered - must be greater than 0.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.3f;

        [Tooltip("Whether the tap gesture will immediately send a begin state when a touch is first down. Default is false.")]
        public bool SendBeginState;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.NumberOfTapsRequired = NumberOfTapsRequired;
            Gesture.ThresholdSeconds = ThresholdSeconds;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.SendBeginState = SendBeginState;
        }
    }
}
