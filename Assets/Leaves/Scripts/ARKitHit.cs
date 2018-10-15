using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.iOS
{
    public class ARKitHit : MonoBehaviour
    {
        public LayerMask collisionLayer         = 1 <<  8;  // ARKitPlane layer
        public LayerMask cameraGridLayer        = 1 <<  9;  // Grid parented to camera layer
        public LayerMask gridLayer              = 1 << 10; // Grids (parented to world) layer
        public LayerMask currentGridLayer       = 1 << 12; // current Grid (parented to world) layer
        public Transform m_HitTransform; // the transform of the raycast hit from screen touch
        public float maxRayDistance = 30.0f;
        [SerializeField] private GameObject PaintingPlane;
        [SerializeField] private bool planePainting = false;

        private PaintManager paintManager;
        private GameObject paintTarget;
        private TransformValues localPlaneTransformValues;
        private float previousRadius; // needed to smooth brush size adjustments 
        private float maxAllowedSizeChange; // also needed to smooth brush size adjustments
        private bool touchIsOverUI;
        private bool hitGrid;

        private void Start()
        {
            maxAllowedSizeChange = 1.3f;
            touchIsOverUI = false;
            hitGrid = false;
            paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
            paintTarget = GameObject.FindWithTag("PaintTarget");
            PaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
            localPlaneTransformValues = new TransformValues();
            localPlaneTransformValues.TransferValues(PaintingPlane.transform);
        }

        bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
        {
            if  (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) { return false; } // don't register touches on the UI

            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);

            if (hitResults.Count > 0)
            {
                foreach (var hitResult in hitResults)  // Why not just get [0] instead of looping?
                {
                    m_HitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform) + Vector3.up * 0.02f; // move above grid slightly
                    m_HitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    if (!planePainting) { PaintPlaneOn(); }
                    return true;
                }
            }
            return false; // no hit results
        }

        private void PaintPlaneOn() // painting on a grid plane
        {
			// Get the current camPaintingPlane that's attached to camera.
            // save the transform again, in case the plane has been moved via the UI
            // localPlaneTransformValues.TransferValues(camPaintingPlane.transform); 
            // Check if painting with device movement is on. If so, remove that brush, and turn painting off
            if (paintManager.paintOn) { paintManager.TogglePaint(); }
            // In case there is still an existing brush, remove it first, so we are starting a brand new stroke
            paintManager.RemoveBrushFromTarget(); // some redundancy with toggle paint

            // move paint target as child
            paintTarget.transform.SetParent(this.transform);
            paintTarget.transform.localPosition = Vector3.zero;
            // add brush to paint target
            paintManager.AddBrushToTarget();
            //make sure PaintOnPlane() is called only once
            planePainting = true;
        }

        private void PaintPlaneOff() 
        {
            // reset the brush
            paintManager.RemoveBrushFromTarget();
            paintTarget.transform.SetParent(Camera.main.transform);
            paintManager.AdjustTargetDistance();

			/******** Try turning this section off, to see what happens if the plane stays with the paintstroke ******/ /*
            // TODO: add conditional for this (no need to call when painting on ARPlanes)
            camPaintingPlane.transform.SetParent(Camera.main.transform);
            // Reset the transform of camPaintingPlane
            camPaintingPlane.transform.localPosition    = localPlaneTransformValues.pos;
            camPaintingPlane.transform.localRotation    = localPlaneTransformValues.rot;
            camPaintingPlane.transform.localScale       = localPlaneTransformValues.scale;
			*********************************************************************************/

            planePainting = false;
            // turn off visibility of plane that is not being currently used
            PaintingPlane.GetComponent<MeshRenderer>().enabled = false;
        }

        void Update()
        {
            #if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;

                    //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
                    //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
            if (Physics.Raycast(ray, out hit, maxRayDistance, currentGridLayer))
                    {
                        //we're going to get the position from the contact point
                        m_HitTransform.position = hit.point;
                        Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                        //and the rotation from the transform of the plane collider
                        m_HitTransform.rotation = hit.transform.rotation;
                    }
                }
            }
            #endif
            var touch = new Touch();
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
            }

            // for Any state of a touch, get the transform of the hit
            if (paintManager.paintOnTouch && Input.touchCount > 0 && !paintManager.ARPlanePainting)
            {
                // First, check if the touch starts on a UI element. If so, set a flag to not start painting
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        touchIsOverUI = true;
                    }
                }

                if (!touchIsOverUI)
                {
                    // Set up 3 raycasts. First will cast against the current layer. 
                    // If nothing on current layer is hit, then raycast against a Grid layer, and find the first grid hit 
                    // (not the camera grid, which will be on a different layer)
                    // If there are no results from that raycast, then raycast against the camera grid layer

                    // TODO: Add a grid property to each paintstroke, and associate that property with that paintstroke
                    // There could/will be multiple paintstrokes for each grid

                    // TODO: using the distance slider automatically changes the priority so that the camera grid layer is checked first.
                    // Also, add an outline instead of a grid, and add a leaf shape, that will have a consistent size (and therefore provide a visual cue
                    // for the distance of the plane from camera (thx. Kathleen T. for the idea)
                    // Also, add shapes with colliders, that can be painted, e.g., a balloon, a ring, a teapot, etc. (thx again Kathleen)
                    // TODO: When paintstroke reaches an edge of a grid, scale the grid so the paintstroke can keep going
                    // TODO: detect whether a grid is close to perpendicular to the camera plane - if so, don't allow paintstroke on that grid

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit = new RaycastHit();

                    // First, raycast against current layer
                    // There is only one current grid at a time, therefore paintstrokes won't jump from grid to grid
                    if (hitGrid || (touch.phase == TouchPhase.Began)) // hitGrid may not have been set to true yet, therefore also check on Began
                    {
                        RaycastPaintingPlane(ray, hit, currentGridLayer);
                    }

                    // If no hit on current layer, then raycast against Grid layer, find the first grid hit, and put it on the current layer
                    if (!hitGrid)
                    {
                        bool checkHit = RaycastPaintingPlane(ray, hit, gridLayer);

                        // if no hit on grid layer, and we are still in Begin phase, then use the grid attached to camera layer (but only if touch phase Began, to avoid generating new planes on touch Move)
                        if (!checkHit && touch.phase == TouchPhase.Began)
                        {
                            RaycastPaintingPlane(ray, hit, cameraGridLayer);
                        }
                    }
                }
            }

            //********* Adjust stroke size based on either touch.radius (finger), or on pressure (styles, e.g. Apple Pencil) ***********
                if (Input.touchCount > 0 && m_HitTransform != null)
                {
                   
                    if (touch.phase == TouchPhase.Began)
                    {
                        previousRadius = touch.radius;
                    }
                    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                    {
                        // Control size of brush with touch
                        // limit max allowed change from previous frame to keep size transitions smooth
                        float allowedRadius = touch.radius;
                        if ((touch.radius - previousRadius) > maxAllowedSizeChange)
                        {
                            allowedRadius = previousRadius + maxAllowedSizeChange;
                        }
                        else if ((touch.radius - previousRadius) < (-1f * maxAllowedSizeChange))
                        {
                          allowedRadius = previousRadius - maxAllowedSizeChange;
                        }
                        previousRadius = allowedRadius; // reset in prep for next frame
                                                    // radius usually in range of 20 to 40, as low as 10, as high as 200
                                                    // attempt to bring brush size to range of 1/4 cm to 10 cm
                        float adjustedRadius = allowedRadius * 0.002f;
                        // if Apple pencil is being used, use the amount of pressure instead
                        if (touch.type == TouchType.Stylus) { adjustedRadius = touch.pressure * 0.04f; }
                        paintManager.brushSize = adjustedRadius * adjustedRadius;
                        paintManager.AdjustBrushSize();


                    //********* For ARKit Plane Detection *********
                    if (!touchIsOverUI)
                    {
                        // This section is needed for painting on AR planes, but interferes with painting on Camera plane (hence the if statement)
                        if (paintManager.ARPlanePainting && touch.phase == TouchPhase.Moved && touch.phase != TouchPhase.Stationary) // try to avoid beginning touch which is off the plane
                        {
                            var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                            ARPoint point = new ARPoint
                            {
                                x = screenPosition.x,
                                y = screenPosition.y
                            };

                            ARHitTestResultType[] resultTypes = { ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent };
                            foreach (ARHitTestResultType resultType in resultTypes)
                            {
                                // returns the 1st point hit casting from screenpoint to an arplane
                                if (HitTestWithResultType(point, resultType)) { return; }
                            }
                        }
                    }
                    //END********* For ARKit Plane Detection *********
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    if (paintTarget.transform.parent.gameObject.CompareTag("PlanePainter") && !EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                    {
                        if (planePainting)
                        {
                            PaintPlaneOff();
                            paintManager.paintOnTouch = true;
                        }
                        PaintingPlane.GetComponent<MeshRenderer>().enabled = false; // should be redundant - in  planepaintoff
                        PaintingPlane.tag = "Grid";
                        PaintingPlane.layer = 10;

                        // now create a new Camera painting grid
                        paintManager.AddPaintingPlaneToCam();
                        PaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
                        /*// ensure that there is no brush attached to the PaintTarget
                            foreach (Transform child in paintTarget.transform) {
                            if (child.name.Contains("Triangle-for-painting")) {
                            Destroy(child);
                        } */
                        }
                        // After each touch is done, reset the touchIsOverUI flag
                        touchIsOverUI = false;
                        hitGrid = false;
                    }
                }
            }

        private bool RaycastPaintingPlane(Ray ray, RaycastHit hit, LayerMask layer)
        {
            if (Physics.Raycast(ray, out hit, maxRayDistance, layer))
            {
                m_HitTransform.position = hit.point;
                m_HitTransform.rotation = hit.transform.rotation;

                // Ideally, these next lines should not be called every frame
                if (!planePainting) { PaintPlaneOn(); }
                PaintingPlane = hit.collider.gameObject;
                PaintingPlane.transform.SetParent(null); // Deparent the plane that's been hit, so it is stationary in world space
                PaintingPlane.tag = "CurrentPaintingObject";
                PaintingPlane.layer = 12; // the int of the current Grid layer                        
                PaintingPlane.GetComponent<MeshRenderer>().enabled = true; // set render to true for current grid only
                hitGrid = true; // flag so that once a touch starts, only that current layer is raycast against, until touch is ended
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}