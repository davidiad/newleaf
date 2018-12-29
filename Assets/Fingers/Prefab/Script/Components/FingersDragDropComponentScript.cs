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
    /// <summary>
    /// Allows a long tap and hold to move an object around and release it at a new point. Add this script to the object to drag.
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Drag and Drop", 0)]
    public class FingersDragDropComponentScript : MonoBehaviour
    {
        [Tooltip("The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera Camera;

        [Tooltip("Whether to bring the object to the front when a gesture executes on it")]
        public bool BringToFront = true;

        [Tooltip("Scale to increase object by when a drag starts. When drags stops, scale is returned to normal.")]
        [Range(1.0f, 1.5f)]
        public float DragScale = 1.1f;

        /// <summary>
        /// Long press gesture to start drag
        /// </summary>
        public LongPressGestureRecognizer LongPressGesture { get; private set; }

        private Rigidbody2D rigidBody;
        private SpriteRenderer spriteRenderer;
        private int startSortOrder;
        private float panZ;
        private Vector3 panOffset;

        private void LongPressGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            FingersPanRotateScaleComponentScript.StartOrResetGesture(r, BringToFront, Camera, gameObject, spriteRenderer, GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject);
            if (r.State == GestureRecognizerState.Began)
            {
                transform.localScale *= DragScale;
                panZ = Camera.WorldToScreenPoint(transform.position).z;
                panOffset = transform.position - Camera.ScreenToWorldPoint(new Vector3(r.FocusX, r.FocusY, panZ));
                if (DragStarted != null)
                {
                    DragStarted.Invoke(this, System.EventArgs.Empty);
                }
            }
            if (r.State == GestureRecognizerState.Executing)
            {
                Vector3 gestureScreenPoint = new Vector3(r.FocusX, r.FocusY, panZ);
                Vector3 gestureWorldPoint = Camera.ScreenToWorldPoint(gestureScreenPoint) + panOffset;
                if (rigidBody == null)
                {
                    transform.position = gestureWorldPoint;
                }
                else
                {
                    rigidBody.MovePosition(gestureWorldPoint);
                }
                if (DragUpdated != null)
                {
                    DragUpdated.Invoke(this, System.EventArgs.Empty);
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                transform.localScale /= DragScale;
                if (spriteRenderer != null && BringToFront)
                {
                    spriteRenderer.sortingOrder = startSortOrder;
                }
                if (DragEnded != null)
                {
                    DragEnded.Invoke(this, System.EventArgs.Empty);
                }
            }
        }

        private void OnEnable()
        {
            this.Camera = (this.Camera == null ? Camera.main : this.Camera);
            LongPressGesture = new LongPressGestureRecognizer();
            LongPressGesture.StateUpdated += LongPressGestureUpdated;
            rigidBody = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                startSortOrder = spriteRenderer.sortingOrder;
            }
            FingersScript.Instance.AddGesture(LongPressGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(LongPressGesture);
            }
        }

        public event System.EventHandler DragStarted;
        public event System.EventHandler DragUpdated;
        public event System.EventHandler DragEnded;
    }
}
