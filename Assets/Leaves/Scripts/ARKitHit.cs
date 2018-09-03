using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;

namespace UnityEngine.XR.iOS
{
    public class ARKitHit : MonoBehaviour
    {
        public Transform m_HitTransform;
        public float maxRayDistance = 30.0f;
        public LayerMask collisionLayer = 1 << 8;  //ARKitPlane layer
        public LayerMask cameraGridLayer = 1 << 9;
        public LayerMask gridLayer = 1 << 10;  //ARKitPlane layer
 //       private PaintOn paintOn; // holds paint status
        private PaintManager paintManager;
        private GameObject paintTarget;
        [SerializeField]private GameObject camPaintingPlane;
        [SerializeField]private bool planePainting = false;
        private TransformValues localPlaneTransformValues;

        private float previousRadius; // needed to smooth brush size adjustments 
        private float maxAllowedSizeChange;

        private void Start()
        {
            maxAllowedSizeChange = 1.3f;
            paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
            paintTarget = GameObject.FindWithTag("PaintTarget");
            camPaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
            localPlaneTransformValues = new TransformValues();
            localPlaneTransformValues.TransferValues(camPaintingPlane.transform);
        }

        bool HitTestWithResultType(ARPoint point, ARHitTestResultType resultTypes)
        {
            if  (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) {
                return false;
            }
            //Debug.Log("IN HIT TEST");

            List<ARHitTestResult> hitResults = UnityARSessionNativeInterface.GetARSessionNativeInterface().HitTest(point, resultTypes);
           
            //if (hitResults.Count > 1)
            //{
            //    // change: use index. If > 1 result, use 1, otherwise use 0
            //    //for (int i = 1; i < hitResults.Count; i++)
            //    //{
            //    // Get not the first hit (which should be cam plane, but the next highest
            //        m_HitTransform.position = UnityARMatrixOps.GetPosition(hitResults[1].worldTransform) + Vector3.up * 0.02f; // move above grid slightly
            //        m_HitTransform.rotation = UnityARMatrixOps.GetRotation(hitResults[1].worldTransform);
                    
            //        if (!planePainting) { PaintPlaneOn(); }

            //        return true;
            //    //}

            //} else 
                if (hitResults.Count > 0)
                // Why not just get [0] instead of looping?
                foreach (var hitResult in hitResults) 
                {
                    m_HitTransform.position = UnityARMatrixOps.GetPosition(hitResult.worldTransform) + Vector3.up*0.02f; // move above grid slightly
                    m_HitTransform.rotation = UnityARMatrixOps.GetRotation(hitResult.worldTransform);
                    //Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));
                    if (!planePainting) { PaintPlaneOn(); }

                    return true;
                }
                
            return false; // no hit results
        }

        private void PaintPlaneOn()
        {
			// Get the current camPaintingPlane that's attached to camera.
			camPaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
            // save the transform again, in case the plane has been moved via the UI
            localPlaneTransformValues.TransferValues(camPaintingPlane.transform); 
            // Check if painting with device movement is on. If so, remove that brush, and turn painting off
            if (paintManager.paintOn) {
                paintManager.TogglePaint();
            }
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

        }

        // Update is called once per frame
        void Update()
        {

            /******************** UNITY EDITOR **************************
#if UNITY_EDITOR   //we will only use this script on the editor side, though there is nothing that would prevent it from working on device
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                //we'll try to hit one of the plane collider gameobjects that were generated by the plugin
                //effectively similar to calling HitTest with ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent
                if (Physics.Raycast(ray, out hit, maxRayDistance, collisionLayer))
                {
                    //we're going to get the position from the contact point
                    m_HitTransform.position = hit.point;

                    //Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                    //and the rotation from the transform of the plane collider
                    m_HitTransform.rotation = hit.transform.rotation;
                }
            }
#else
*****************************************************************************/

            // detect hit on plane in front of camera, or one of the other grid planes associated with a paintstroke

            // Any state of a touch, get the transform of the hit
            if (paintManager.paintOnTouch && Input.touchCount > 0 && !paintManager.ARPlanePainting)
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // Block if over UI element
                { 
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
                    if (Physics.Raycast(ray, out hit, maxRayDistance, gridLayer)) 
                    {
                        //we're going to get the position from the contact point
                        m_HitTransform.position = hit.point;
                        Debug.Log("PAINTONTOUCH");
                        Debug.Log(string.Format("x:{0:0.######} y:{1:0.######} z:{2:0.######}", m_HitTransform.position.x, m_HitTransform.position.y, m_HitTransform.position.z));

                        //and the rotation from the transform of the plane collider
                        m_HitTransform.rotation = hit.transform.rotation;
                        if (!planePainting) { PaintPlaneOn(); }
                        // Deparent the plane that's been hit, so it is stationary in world space

                        camPaintingPlane.transform.SetParent(null);
                        // Change tag to Grid. There will be only one camPaintingPlane at a time, but could be many Grids
                        // A new camPaintingPlane will be created when this paintstroke is ended
                        camPaintingPlane.tag = "Grid";
                    }
                    // No other grids found, so cast against the grid attached to the camera
                    // Assuming cam grid is found (usually it will be), a new grid will be created, and then detached from the camera
                    else if (Physics.Raycast(ray, out hit, maxRayDistance, cameraGridLayer)) 
                    {
                        m_HitTransform.position = hit.point;
                        m_HitTransform.rotation = hit.transform.rotation;
                        // etc. TK
                    }
                }
            }

            if (Input.touchCount > 0 && m_HitTransform != null)
            {
                var touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began) {
					
                    previousRadius = touch.radius;
                }
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    // Control size of brush with touch
                    Debug.Log("Touch Radius: " + touch.radius);

                    // limit max allowed change from previous frame to keep size transitions smooth
                    float allowedRadius = touch.radius;
                    if ((touch.radius - previousRadius) > maxAllowedSizeChange) {
                        allowedRadius = previousRadius + maxAllowedSizeChange;
                    } else if ((touch.radius - previousRadius) < (-1f * maxAllowedSizeChange)) {
                        allowedRadius = previousRadius - maxAllowedSizeChange;
                    }
                    previousRadius = allowedRadius; // reset in prep for next frame
                    // radius usually in range of 20 to 40, as low as 10, as high as 200
                    // attempt to bring brush size to range of 1/4 cm to 10 cm
                    float adjustedRadius = allowedRadius * 0.002f;
                    // if Apple pencil is being used, use the amount of pressure instead
                    if (touch.type == TouchType.Stylus) { adjustedRadius = touch.pressure * 0.04f; }
                    paintManager.brushSize = adjustedRadius * adjustedRadius;
                    Debug.Log("brushSize: " + adjustedRadius);

                    paintManager.AdjustBrushSize();

            // This section is needed for painting on AR planes, but interferes with painting on Camera plane (hence the if statement)
            if (paintManager.ARPlanePainting && touch.phase == TouchPhase.Moved && touch.phase != TouchPhase.Stationary) // try to avoid beginning touch which is off the plane
                    {
                        var screenPosition = Camera.main.ScreenToViewportPoint(touch.position);
                        ARPoint point = new ARPoint
                        {
                            x = screenPosition.x,
                            y = screenPosition.y
                        };

                        // prioritize results types
                        ARHitTestResultType[] resultTypes = {
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingGeometry,
                        ARHitTestResultType.ARHitTestResultTypeExistingPlaneUsingExtent, 
                        // if you want to use infinite planes use this:
                        //ARHitTestResultType.ARHitTestResultTypeExistingPlane,
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedHorizontalPlane, 
                        //ARHitTestResultType.ARHitTestResultTypeEstimatedVerticalPlane, 
                        //ARHitTestResultType.ARHitTestResultTypeFeaturePoint
                    };

                        foreach (ARHitTestResultType resultType in resultTypes)
                        {
                            // returns the 1st (or 2nd) point hit casting from screenpoint to an arplane
                            if (HitTestWithResultType(point, resultType))
                            {
                                return;
                            }
                        }
                    }
                } else if (touch.phase == TouchPhase.Ended) 
                {
            Debug.Log("000000000000000");
                    Debug.Log("paintManager.paintOnTouch: " + paintManager.paintOnTouch);
                    if (paintTarget.transform.parent.gameObject.CompareTag("PlanePainter"))
                    {
                        Debug.Log("111111111111");
                        //if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId)) // don't call when over UI
                        //{
                            Debug.Log("3333333333");
                        if (planePainting) { 
                            PaintPlaneOff(); 
                            paintManager.paintOnTouch = true; 
                        }
                        paintManager.AddPaintingPlaneToCam();
                        /*
                        // ensure that there is no brush attached to the PaintTarget
                        foreach (Transform child in paintTarget.transform) {
                            if (child.name.Contains("Triangle-for-painting")) {
                                Destroy(child);
                            }
                        }
                        */
                    }
                }
            }
//#endif

        }


    }
}

