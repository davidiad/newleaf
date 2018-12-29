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
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DigitalRubyShared
{
    /// <summary>
    /// Allows two finger pan, scale and rotate on a game object
    /// </summary>
    [AddComponentMenu("Fingers Gestures/Component/Pan-Rotate-Scale", 4)]
    public class FingersPanRotateScaleComponentScript : MonoBehaviour
    {
        [Tooltip("The camera to use to convert screen coordinates to world coordinates. Defaults to Camera.main.")]
        public Camera Camera;

        [Tooltip("Whether to bring the object to the front when a gesture executes on it")]
        public bool BringToFront = true;

        [Tooltip("Whether the gestures in this script can execute simultaneously with all other gestures.")]
        public bool AllowExecutionWithAllGestures;

        [Tooltip("The mode to execute in, can be require over game object or allow on any game object.")]
        public GestureRecognizerComponentScriptBase.GestureObjectMode Mode = GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject;

        [Tooltip("The minimum and maximum scale. 0 for no limits.")]
        public Vector2 MinMaxScale;

        [Tooltip("Whether to add a double tap to reset the transform of the game object this script is on.")]
        public bool DoubleTapToReset;

        /// <summary>
        /// Allow moving the target
        /// </summary>
        public PanGestureRecognizer PanGesture { get; private set; }

        /// <summary>
        /// Allow scaling the target
        /// </summary>
        public ScaleGestureRecognizer ScaleGesture { get; private set; }

        /// <summary>
        /// Allow rotating the target
        /// </summary>
        public RotateGestureRecognizer RotateGesture { get; private set; }

        /// <summary>
        /// The double tap gesture or null if DoubleTapToReset was false when this script started up
        /// </summary>
        public TapGestureRecognizer DoubleTapGesture { get; private set; }

        private Rigidbody2D rigidBody2D;
        private Rigidbody rigidBody;
        private SpriteRenderer spriteRenderer;
        private CanvasRenderer canvasRenderer;
        private Transform _transform;
        private int startSortOrder;
        private float panZ;
        private Vector3 panOffset;

        private struct SavedState
        {
            public Vector3 Scale;
            public Quaternion Rotation;
        }
        private readonly Dictionary<Transform, SavedState> savedStates = new Dictionary<Transform, SavedState>();

        private static readonly List<RaycastResult> captureRaycastResults = new List<RaycastResult>();

        public static GameObject StartOrResetGesture(DigitalRubyShared.GestureRecognizer r, bool bringToFront, Camera camera, GameObject obj, SpriteRenderer spriteRenderer, GestureRecognizerComponentScriptBase.GestureObjectMode mode)
        {
            GameObject result = null;
            if (r.State == GestureRecognizerState.Began)
            {
                if ((result = GestureIntersectsObject(r, camera, obj, mode)) != null)
                {
                    SpriteRenderer _spriteRenderer;
                    if (bringToFront && (_spriteRenderer = result.GetComponent<SpriteRenderer>()) != null)
                    {
                        _spriteRenderer.sortingOrder = 1000;
                    }
                }
                else
                {
                    r.Reset();
                }
            }
            return result;
        }

        private static int RaycastResultCompare(RaycastResult r1, RaycastResult r2)
        {
            SpriteRenderer rend1 = r1.gameObject.GetComponent<SpriteRenderer>();
            if (rend1 != null)
            {
                SpriteRenderer rend2 = r2.gameObject.GetComponent<SpriteRenderer>();
                if (rend2 != null)
                {
                    int comp = rend2.sortingLayerID.CompareTo(rend1.sortingLayerID);
                    if (comp == 0)
                    {
                        comp = rend2.sortingOrder.CompareTo(rend1.sortingOrder);
                    }
                    return comp;
                }
            }
            return r2.gameObject.transform.GetSiblingIndex().CompareTo(r1.gameObject.transform.GetSiblingIndex());
        }

        private static GameObject GestureIntersectsObject(DigitalRubyShared.GestureRecognizer r, Camera camera, GameObject obj, GestureRecognizerComponentScriptBase.GestureObjectMode mode)
        {
            captureRaycastResults.Clear();
            PointerEventData p = new PointerEventData(EventSystem.current);
            p.Reset();
            p.position = new Vector2(r.FocusX, r.FocusY);
            p.clickCount = 1;
            EventSystem.current.RaycastAll(p, captureRaycastResults);
            captureRaycastResults.Sort(RaycastResultCompare);

            foreach (RaycastResult result in captureRaycastResults)
            {
                if (result.gameObject == obj)
                {
                    return result.gameObject;
                }
                else if (result.gameObject.GetComponent<Collider>() != null ||
                    result.gameObject.GetComponent<Collider2D>() != null ||
                    result.gameObject.GetComponent<FingersPanRotateScaleComponentScript>() != null)
                {
                    if (mode == GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
                    {
                        return result.gameObject;
                    }

                    // blocked by a collider or another gesture, bail
                    break;
                }
            }
            return null;
        }

        private void PanGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            GameObject obj = StartOrResetGesture(r, BringToFront, Camera, gameObject, spriteRenderer, Mode);
            if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (PanGesture.ReceivedAdditionalTouches)
                {
                    panZ = Camera.WorldToScreenPoint(_transform.position).z;
                    panOffset = _transform.position - Camera.ScreenToWorldPoint(new Vector3(r.FocusX, r.FocusY, panZ));
                }
                Vector3 gestureScreenPoint = new Vector3(r.FocusX, r.FocusY, panZ);
                Vector3 gestureWorldPoint = Camera.ScreenToWorldPoint(gestureScreenPoint) + panOffset;
                if (rigidBody != null)
                {
                    rigidBody.MovePosition(gestureWorldPoint);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MovePosition(gestureWorldPoint);
                }
                else if (canvasRenderer != null)
                {
                    _transform.position = gestureScreenPoint;
                }
                else
                {
                    _transform.position = gestureWorldPoint;
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                if (spriteRenderer != null && BringToFront)
                {
                    spriteRenderer.sortingOrder = startSortOrder;
                }
                ClearStartState();
            }
        }

        private void ScaleGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            GameObject obj = StartOrResetGesture(r, BringToFront, Camera, gameObject, spriteRenderer, Mode);
            if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                // assume uniform scale
                float scale = _transform.localScale.x * ScaleGesture.ScaleMultiplier;

                if (MinMaxScale.x > 0.0f && MinMaxScale.y >= MinMaxScale.x)
                {
                    scale = Mathf.Clamp(scale, MinMaxScale.x, MinMaxScale.y);
                }

                // don't mess with z scale for 2D
                float zScale = (rigidBody2D == null && spriteRenderer == null && canvasRenderer == null ? scale : _transform.localScale.z);
                _transform.localScale = new Vector3(scale, scale, zScale);
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void RotateGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            GameObject obj = StartOrResetGesture(r, BringToFront, Camera, gameObject, spriteRenderer, Mode);
            if (r.State == GestureRecognizerState.Began)
            {
                SetStartState(r, obj, false);
            }
            else if (r.State == GestureRecognizerState.Executing && _transform != null)
            {
                if (rigidBody != null)
                {
                    float angle = RotateGesture.RotationDegreesDelta;
                    Quaternion rotation = Quaternion.AngleAxis(angle, Camera.transform.forward);
                    rigidBody.MoveRotation(rigidBody.rotation * rotation);
                }
                else if (rigidBody2D != null)
                {
                    rigidBody2D.MoveRotation(rigidBody2D.rotation + RotateGesture.RotationDegreesDelta);
                }
                else if (canvasRenderer != null)
                {
                    _transform.Rotate(Vector3.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
                else
                {
                    _transform.Rotate(Camera.transform.forward, RotateGesture.RotationDegreesDelta, Space.Self);
                }
            }
            else if (r.State == GestureRecognizerState.Ended)
            {
                ClearStartState();
            }
        }

        private void DoubleTapGestureUpdated(DigitalRubyShared.GestureRecognizer r)
        {
            if (r.State == GestureRecognizerState.Ended)
            {
                GameObject obj = GestureIntersectsObject(r, Camera, gameObject, Mode);
                SavedState state;
                if (obj != null && savedStates.TryGetValue(obj.transform, out state))
                {
                    obj.transform.rotation = state.Rotation;
                    obj.transform.localScale = state.Scale;
                    savedStates.Remove(obj.transform);
                }
            }
        }

        private void ClearStartState()
        {
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return;
            }
            else if (PanGesture.State != GestureRecognizerState.Executing &&
                RotateGesture.State != GestureRecognizerState.Executing &&
                ScaleGesture.State != GestureRecognizerState.Executing)
            {
                rigidBody2D = null;
                rigidBody = null;
                spriteRenderer = null;
                canvasRenderer = null;
                _transform = null;
            }
        }

        private bool SetStartState(DigitalRubyShared.GestureRecognizer gesture, GameObject obj, bool force)
        {
            if (!force && Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                return false;
            }
            else if (obj == null)
            {
                ClearStartState();
                return false;
            }
            else if (_transform == null)
            {
                rigidBody2D = obj.GetComponent<Rigidbody2D>();
                rigidBody = obj.GetComponent<Rigidbody>();
                spriteRenderer = obj.GetComponent<SpriteRenderer>();
                canvasRenderer = obj.GetComponent<CanvasRenderer>();
                if (spriteRenderer != null)
                {
                    startSortOrder = spriteRenderer.sortingOrder;
                }
                _transform = (rigidBody == null ? (rigidBody2D == null ? obj.transform : rigidBody2D.transform) : rigidBody.transform);
                if (DoubleTapToReset && !savedStates.ContainsKey(_transform))
                {
                    savedStates[_transform] = new SavedState { Rotation = _transform.rotation, Scale = _transform.localScale };
                }
            }
            else if (_transform != obj.transform)
            {
                if (gesture != null)
                {
                    gesture.Reset();
                }
                return false;
            }
            return true;
        }

        private void OnEnable()
        {
            this.Camera = (this.Camera == null ? Camera.main : this.Camera);
            PanGesture = new PanGestureRecognizer { MaximumNumberOfTouchesToTrack = 2, ThresholdUnits = 0.01f };
            PanGesture.StateUpdated += PanGestureUpdated;
            ScaleGesture = new ScaleGestureRecognizer();
            ScaleGesture.StateUpdated += ScaleGestureUpdated;
            RotateGesture = new RotateGestureRecognizer();
            RotateGesture.StateUpdated += RotateGestureUpdated;
            if (Mode != GestureRecognizerComponentScriptBase.GestureObjectMode.AllowOnAnyGameObjectViaRaycast)
            {
                SetStartState(null, gameObject, true);
            }
            if (DoubleTapToReset)
            {
                DoubleTapGesture = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                DoubleTapGesture.StateUpdated += DoubleTapGestureUpdated;
            }
            if (AllowExecutionWithAllGestures)
            {
                PanGesture.AllowSimultaneousExecutionWithAllGestures();
                RotateGesture.AllowSimultaneousExecutionWithAllGestures();
                ScaleGesture.AllowSimultaneousExecutionWithAllGestures();
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecutionWithAllGestures();
                }
            }
            else
            {
                PanGesture.AllowSimultaneousExecution(ScaleGesture);
                PanGesture.AllowSimultaneousExecution(RotateGesture);
                ScaleGesture.AllowSimultaneousExecution(RotateGesture);
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.AllowSimultaneousExecution(ScaleGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(RotateGesture);
                    DoubleTapGesture.AllowSimultaneousExecution(PanGesture);
                }
            }
            if (Mode == GestureRecognizerComponentScriptBase.GestureObjectMode.RequireIntersectWithGameObject)
            {
                RotateGesture.PlatformSpecificView = gameObject;
                PanGesture.PlatformSpecificView = gameObject;
                ScaleGesture.PlatformSpecificView = gameObject;
                if (DoubleTapGesture != null)
                {
                    DoubleTapGesture.PlatformSpecificView = gameObject;
                }
            }
            FingersScript.Instance.AddGesture(PanGesture);
            FingersScript.Instance.AddGesture(ScaleGesture);
            FingersScript.Instance.AddGesture(RotateGesture);
            FingersScript.Instance.AddGesture(DoubleTapGesture);
        }

        private void OnDisable()
        {
            if (FingersScript.HasInstance)
            {
                FingersScript.Instance.RemoveGesture(PanGesture);
                FingersScript.Instance.RemoveGesture(ScaleGesture);
                FingersScript.Instance.RemoveGesture(RotateGesture);
                FingersScript.Instance.RemoveGesture(DoubleTapGesture);
            }
        }
    }
}
