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
    [AddComponentMenu("Fingers Gestures/Gesture/Scale Gesture (Two Touches)", 7)]
    public class ScaleGestureRecognizerComponentScript : GestureRecognizerComponentScript<ScaleGestureRecognizer>
    {
        [Header("Scale gesture properties")]
        [Tooltip("Additional multiplier for ScaleMultiplier. This will making scaling happen slower or faster.")]
        [Range(0.01f, 10.0f)]
        public float ZoomSpeed = 3.0f;

        [Tooltip("How many units the distance between the fingers must increase or decrease from the start distance to begin executing.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.15f;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.ZoomSpeed = ZoomSpeed;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack =
                Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack = 2;
        }
    }
}
