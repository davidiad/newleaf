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
    public class DemoScriptPan : MonoBehaviour
    {
        public FingersScript FingersScript;
        public UnityEngine.UI.Text log;
        public UnityEngine.UI.ScrollRect logView;

        private void Log(string text, params object[] args)
        {
            string logText = string.Format(text, args);
            log.text += logText + System.Environment.NewLine;
            Debug.Log(logText);
            logView.normalizedPosition = Vector2.zero;
        }

        private void Start()
        {
            PanGestureRecognizer pan = new PanGestureRecognizer();
            pan.StateUpdated += Pan_Updated;
            pan.MaximumNumberOfTouchesToTrack = 2;
            FingersScript.AddGesture(pan);

            TapGestureRecognizer tap = new TapGestureRecognizer();
            tap.StateUpdated += Tap_StateUpdated;
            tap.ClearTrackedTouchesOnEndOrFail = true;
            tap.AllowSimultaneousExecution(pan);
            FingersScript.AddGesture(tap);
        }

        private void Tap_StateUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Ended)
            {
                Log("Tap gesture, state: {0}, position: {1},{2}", gesture.State, gesture.FocusX, gesture.FocusY);
            }
        }

        private void Pan_Updated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.CurrentTrackedTouches.Count != 0)
            {
                GestureTouch t = gesture.CurrentTrackedTouches[0];
                Log("Pan gesture, state: {0}, position: {1},{2} -> {3},{4}", gesture.State, t.PreviousX, t.PreviousY, t.X, t.Y);
            }
        }
    }
}