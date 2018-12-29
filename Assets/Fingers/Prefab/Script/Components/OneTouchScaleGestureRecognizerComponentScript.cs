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
    [AddComponentMenu("Fingers Gestures/Gesture/Scale Gesture (One Touch)", 6)]
    public class OneTouchScaleGestureRecognizerComponentScript : GestureRecognizerComponentScript<OneTouchScaleGestureRecognizer>
    {
        [Header("One touch scale gesture properties")]
        [Tooltip("Additional multiplier for ScaleMultiplier. This will making scaling happen slower or faster.")]
        [Range(-10.0f, 10.0f)]
        public float ZoomSpeed = -0.2f;

        [Tooltip("The threshold in units that the touch must move to start the gesture.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.15f;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.ZoomSpeed = ZoomSpeed;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack =
                Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack = 1;
        }
    }
}
