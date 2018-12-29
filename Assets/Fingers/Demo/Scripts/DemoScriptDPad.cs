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
    public class DemoScriptDPad : MonoBehaviour
    {
        [Tooltip("Fingers DPad Script")]
        public FingersDPadScript DPadScript;

        [Tooltip("Object to move with the dpad")]
        public GameObject Mover;

        [Tooltip("Units per second to move the square with dpad")]
        public float Speed = 250.0f;

        [Tooltip("Whether dpad moves to touch start location")]
        public bool MoveDPadToGestureStartLocation;

        private Vector3 startPos;

        private void Awake()
        {
            DPadScript.DPadItemTapped = DPadTapped;
            DPadScript.DPadItemPanned = DPadPanned;
            startPos = Mover.transform.position;
            DPadScript.MoveDPadToGestureStartLocation = MoveDPadToGestureStartLocation;
        }

        private void DPadTapped(FingersDPadScript script, FingersDPadItem item, TapGestureRecognizer gesture)
        {
            if ((item & FingersDPadItem.Center) != FingersDPadItem.None)
            {
                Mover.transform.position = startPos;
            }
        }

        private void DPadPanned(FingersDPadScript script, FingersDPadItem item, PanGestureRecognizer gesture)
        {
            Vector3 pos = Mover.transform.position;
            float move = Speed * Time.deltaTime;
            if ((item & FingersDPadItem.Right) != FingersDPadItem.None)
            {
                pos.x += move;
            }
            if ((item & FingersDPadItem.Left) != FingersDPadItem.None)
            {
                pos.x -= move;
            }
            if ((item & FingersDPadItem.Up) != FingersDPadItem.None)
            {
                pos.y += move;
            }
            if ((item & FingersDPadItem.Down) != FingersDPadItem.None)
            {
                pos.y -= move;
            }
            Mover.transform.position = pos;
        }
    }
}
