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
    [AddComponentMenu("Fingers Gestures/Gesture/Long Press Gesture", 3)]
    public class LongPressGestureRecognizerComponentScript : GestureRecognizerComponentScript<LongPressGestureRecognizer>
    {
        [Header("Long press gesture properties")]
        [Tooltip("The number of seconds that the touch must stay down to begin executing.")]
        [Range(0.01f, 1.0f)]
        public float MinimumDurationSeconds = 0.6f;

        [Tooltip("How many units away the long press can move before failing. After the long press begins, " +
            "it is allowed to move any distance and stay executing.")]
        [Range(0.01f, 1.0f)]
        public float ThresholdUnits = 0.35f;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.MinimumDurationSeconds = MinimumDurationSeconds;
            Gesture.ThresholdUnits = ThresholdUnits;
        }
    }
}
