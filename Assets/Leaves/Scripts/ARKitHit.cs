﻿using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.iOS
{
    public class ARKitHit : MonoBehaviour
    {
        public LayerMask collisionLayer  = 1 << 8;  // ARKitPlane layer
        public LayerMask cameraGridLayer = 1 << 9;  // Grid parented to camera layer
        public LayerMask gridLayer       = 1 << 10; // Grids (parented to world) layer
        public Transform m_HitTransform; // the transform of the raycast hit from screen touch
        public float maxRayDistance = 30.0f;
        [SerializeField] private GameObject camPaintingPlane;
        [SerializeField] private bool planePainting = false;

        private PaintManager paintManager;
        private GameObject paintTarget;
        private TransformValues localPlaneTransformValues;
        private float previousRadius; // needed to smooth brush size adjustments 
        private float maxAllowedSizeChange; // also needed to smooth brush size adjustments
        private bool touchIsOverUI;

        private void Start()
        {
            maxAllowedSizeChange = 1.3f;
            touchIsOverUI = false;
            paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
            paintTarget = GameObject.FindWithTag("PaintTarget");
            camPaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
            localPlaneTransformValues = new TransformValues();
            localPlaneTransformValues.TransferValues(camPaintingPlane.transform);
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
            camPaintingPlane.GetComponent<MeshRenderer>().enabled = false;
        }

        void Update()
        {
            var touch = new Touch();
            if (Input.touchCount > 0)
            {
                touch = Input.GetTouch(0);
            }

            // for Any state of a touch, get the transform of the hit
            if (paintManager.paintOnTouch && Input.touchCount > 0 && !paintManager.ARPlanePainting)
            {
                if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                {
                    if (touch.phase == TouchPhase.Began)
                    {
                        touchIsOverUI = true;
                    }
                }
                //if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // Block if over UI element
                if (!touchIsOverUI)
                {
                    Debug.Log("YYY");
                    //TODO: set up Begin separeatly from other states, so can check for tag of collider (prevent jumping from one grid to another)

                    //TODO: set up current object on a separate physics layer
                    // once a touch starts, only that current layer is raycast against, until touch is ended

                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    // try to hit plane collider gameobjects attached to camera
                    // effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent

                    //TODO: Set up 2 raycasts. First will raycast against a Grid layer, and find the first grid hit 
                    // (not the camera grid, which will be on a different layer)
                    // If there are no results from that first raycast, then raycast against the camera grid layer

                    // first, check if there are any grids (not cam grid) being hit
                    // If a grid is found, use that exisiting grid
                    // TODO: Add a grid property to each paintstroke, and associate that property with that paintstroke
                    // There could/will be mulitple paintstrokes for each grid

                    // Raycast against Grid layer, and find the first grid hit
                    if (Physics.Raycast(ray, out hit, maxRayDistance, gridLayer))
                    {
                        Debug.Log("ZZZ");
                        if (touch.phase == TouchPhase.Began)
                        {
                            hit.collider.gameObject.tag = "CurrentPaintingObject";
                            Debug.Log("AAA");
                        }
                        if (hit.collider.CompareTag("CurrentPaintingObject"))
                        {
                            Debug.Log("BBB");
                            // Get the position from the contact point
                            m_HitTransform.position = hit.point;

                            // and the rotation from the transform of the plane collider
                            m_HitTransform.rotation = hit.transform.rotation;
                            hit.collider.gameObject.GetComponent<MeshRenderer>().enabled = true;

                            if (!planePainting) { PaintPlaneOn(); }
                        }

                    }                         
                    else if (touch.phase == TouchPhase.Moved) // hit was null relative to grid layer, or it hit an object not tagged as CurrentPaintingObject
                    {
                        Debug.Log("CCC");
                        // The touch continues, but we have left the active grid.
                        // As a higher priority to painting with a new plane attached to camera,
                        // To avoid disconcerting jumps to other grids, end this stroke

                        camPaintingPlane.tag = "Grid";
                        camPaintingPlane.GetComponent<MeshRenderer>().enabled = false;
                        PaintPlaneOff();
                    }
                    // No other grids found, so cast against the grid attached to the camera
                    // Assuming cam grid is found (usually it will be), a new grid will be created, and then detached from the camera
                    // Check first if we are in the middle of painting on a grid, and then run off the grid. In that case, end the stroke
                    // to avoid jumping to another grid far away

                    // no hit on grid layer and we are still in Begin phase, then use camera grid layer
                    else if (Physics.Raycast(ray, out hit, maxRayDistance, cameraGridLayer) && (touch.phase == TouchPhase.Began))
                        {
                            Debug.Log("DDD");
                            m_HitTransform.position = hit.point;
                            m_HitTransform.rotation = hit.transform.rotation;
                            if (!planePainting) { PaintPlaneOn(); }

                            // Deparent the plane that's been hit, so it is stationary in world space
                            hit.collider.gameObject.transform.SetParent(null);
                            // camPaintingPlane.transform.SetParent(null);
                            // Convert camPaintingPlane to GridPlane. There will be only one camPaintingPlane at a time, but could be many Grids
                            // A new camPaintingPlane will be created when this paintstroke is ended
                            camPaintingPlane.tag = "CurrentPaintingObject";
                            camPaintingPlane.layer = 10; // the int of the Grid layer
                            // set render to true for current grid only
                            camPaintingPlane.GetComponent<MeshRenderer>().enabled = true;
                        }
                    
                }
            }

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
                        camPaintingPlane.GetComponent<MeshRenderer>().enabled = false; // should be redundant - in  planepaintoff
                        camPaintingPlane.tag = "Grid";

                        // now create a new Camera painting grid
                        paintManager.AddPaintingPlaneToCam();
                        camPaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
                        /*// ensure that there is no brush attached to the PaintTarget
                            foreach (Transform child in paintTarget.transform) {
                            if (child.name.Contains("Triangle-for-painting")) {
                            Destroy(child);
                        } */
                        }
                        // After each touch is done, reset the touchIsOverUI flag
                        touchIsOverUI = false;
                    }
                }
            }
        }
}