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
    [AddComponentMenu("Fingers Gestures/Gesture/Pan Gesture", 1)]
    public class PanGestureRecognizerComponentScript : GestureRecognizerComponentScript<PanGestureRecognizer>
    {
        [Header("Pan gesture properties")]
        [Tooltip("How many units away the pan must move to execute.")]
        [Range(0.0f, 1.0f)]
        public float ThresholdUnits = 0.2f;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.ThresholdUnits = ThresholdUnits;
        }
    }
}
