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
    [AddComponentMenu("Fingers Gestures/Gesture/Rotate Gesture (Two Touches)", 5)]
    public class RotateGestureRecognizerComponentScript : GestureRecognizerComponentScript<RotateGestureRecognizer>
    {
        [Header("Rotate gesture properties")]
        [Tooltip("Angle threshold in radians that must be met before rotation starts - this is the amount of rotation that must happen to start the gesture.")]
        [Range(0.01f, 0.5f)]
        public float AngleThreshold = 0.05f;

        [Tooltip("The gesture focus must change distance by this number of units from the start focus in order to start.")]
        [Range(0.0f, 1.0f)]
        public float ThresholdUnits;

        protected override void OnEnable()
        {
            base.OnEnable();

            Gesture.AngleThreshold = AngleThreshold;
            Gesture.ThresholdUnits = ThresholdUnits;
            Gesture.MinimumNumberOfTouchesToTrack = MinimumNumberOfTouchesToTrack =
                Gesture.MaximumNumberOfTouchesToTrack = MaximumNumberOfTouchesToTrack = 2;
        }
    }
}
