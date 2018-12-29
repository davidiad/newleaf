//
// Fingers Gestures
// (c) 2015 Digital Ruby, LLC
// Source code may be used for personal or commercial projects.
// Source code may NOT be redistributed or sold.
// 

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace DigitalRubyShared
{
    public class DemoScriptOneFinger : MonoBehaviour
    {
        public GameObject RotateIcon;
        public GameObject ScaleIcon;
        public GameObject Earth;

        private OneTouchRotateGestureRecognizer rotationGesture = new OneTouchRotateGestureRecognizer();
        private OneTouchScaleGestureRecognizer scaleGesture = new OneTouchScaleGestureRecognizer();

        private bool GestureIntersectsSprite(DigitalRubyShared.GestureRecognizer g, GameObject obj)
        {
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(g.StartFocusX, g.StartFocusY, -Camera.main.transform.position.z));
            Collider2D col = Physics2D.OverlapPoint(worldPos);
            return (col != null && col.gameObject != null && col.gameObject == obj);
        }

        private void RotationGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began && !GestureIntersectsSprite(gesture, RotateIcon))
            {
                gesture.Reset();
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
                Earth.transform.Rotate(0.0f, 0.0f, rotationGesture.RotationDegreesDelta);
            }
        }

        private void ScaleGestureUpdated(DigitalRubyShared.GestureRecognizer gesture)
        {
            if (gesture.State == GestureRecognizerState.Began && !GestureIntersectsSprite(gesture, ScaleIcon))
            {
                gesture.Reset();
            }
            else if (gesture.State == GestureRecognizerState.Executing)
            {
                Earth.transform.localScale *= scaleGesture.ScaleMultiplier;
            }
        }
        
        private void Start()
        {
            FingersScript.Instance.AddGesture(rotationGesture);
            rotationGesture.StateUpdated += RotationGestureUpdated;

            FingersScript.Instance.AddGesture(scaleGesture);
            scaleGesture.StateUpdated += ScaleGestureUpdated;

            // if you wanted to rotate the earth from the center of the earth rather than the button, you could do this:
            //Vector3 screenPos = Camera.main.WorldToScreenPoint(Earth.transform.position);
            //rotationGesture.AnglePointOverrideX = screenPos.x;
            //rotationGesture.AnglePointOverrideY = screenPos.y;
        }
                
        private void Update()
        {

        }
    }
}