﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ara; // 3rd party Trail Renderer

public class PaintManager : MonoBehaviour
{
    public Button StopPaintingButton; // only for editor
    public GameObject paintingPlane;
    public GameObject paintBrushPrefab;
    public Button onoff;
    public ColorJoystick colorJoystick;
    public Vector3 paintPosition;
    public float brushSize;
    public float strokeThickness; // multiplier, sets overall thickness of trail
    public float joystickSensitivity = 0.0125f;
    public bool paintOnTouch;
    public bool ARPlanePainting;
    public List<PaintStroke> paintStrokesList;
    public bool paintOn;
    public AraTrail currentAraTrail;
    [SerializeField] private GameObject paintTarget;
    [SerializeField] Camera mainCam;
    [SerializeField] private float paintWait; // time to wait before adding next vertex when painting trail

    private LeavesManager leavesManager;
    private GameObject targetSliderGO;
    private Slider targetSlider;
    private Slider paintSlider;
    private Slider brushSizeSlider;
    private bool newPaintVertices;
    private Color paintColor;
    private Material[] brushColorMats;
    private Vector3 previousPosition;
    private float colorDarken = 0.65f; // amount to darken the outer rings of the cursor
    private Vector3 colorInput;
    private float hue;
    private float planeScale = 0.08f;
    private GameObject paintBrush;
    private CanvasGroup paintButtonGroup;
    private GameObject paintstrokeParent;
    private GameObject stopPaintingButton;

    void Start()
    {
        paintWait = 0.05f; // factor to control speed of re-drawing saved paintstrokes
        brushSize = 0.005f; // in meters
        strokeThickness = 1f;
        paintOn = false;
        paintOnTouch = true;
        newPaintVertices = false;
        paintStrokesList = new List<PaintStroke>();
        paintColor = Color.blue;
        brushColorMats = GameObject.FindWithTag("BrushColor").GetComponent<Renderer>().materials;
        leavesManager = GameObject.FindWithTag("LeavesManager").GetComponent<LeavesManager>();
        paintTarget = GameObject.FindWithTag("PaintTarget");
        paintSlider = GameObject.FindWithTag("PaintSlider").GetComponent<Slider>();
        brushSizeSlider = GameObject.FindWithTag("SizeSlider").GetComponent<Slider>();
        targetSliderGO = GameObject.FindWithTag("TargetSlider");
        targetSlider = targetSliderGO.GetComponent<Slider>();
        paintPosition = leavesManager.paintPosition;
        AdjustTargetDistance();
        AdjustPaintColor(); // set the color to what the color slider is set to
        paintButtonGroup = onoff.GetComponent<CanvasGroup>();
        paintButtonGroup.alpha = 0.4f;
        SetHue(paintColor);
        paintstrokeParent = new GameObject("paintstrokeParent"); // initially at (0,0,0)
        stopPaintingButton = GameObject.FindWithTag("StopPainting");
#if UNITY_EDITOR
        stopPaintingButton.SetActive(true);
#else
        stopPaintingButton.SetActive(false);
#endif
    }
 
    void Update()
    {
        colorInput = colorJoystick.GetInputDirection();
        if (colorInput != Vector3.zero)
        {
            UpdateSV();
        }
    }

    // to keep hue constant when setting S and V
    private void SetHue(Color color) {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        hue = H;
    }

    public void AddPaintingPlaneToCam() {
        // check if cam has painting plane
        if (GameObject.FindWithTag("CamPaintingPlane") == null) {
            // if not, instantiate painting plane, and add as a child to main cam
            GameObject newPaintingPlane = Instantiate(paintingPlane, Vector3.zero, Quaternion.identity);
            newPaintingPlane.transform.parent = mainCam.transform;
            newPaintingPlane.transform.localPosition = new Vector3(0f, 0f, 0.3f);
            newPaintingPlane.transform.localRotation = Quaternion.identity;
            newPaintingPlane.transform.localScale = new Vector3(planeScale, planeScale, planeScale); 
        }
    }

    // uses paint slider
    public void AdjustPaintColor() {
        if (paintSlider)
        {
            Gradient paintGradient = paintSlider.GetComponent<PaintGradient>().gradient;
            Color tempColor = paintGradient.Evaluate(paintSlider.value);
            SetHue(tempColor); // get the hue from the gradient slider
            UpdateSV(); // Update paintColor with new hue, and previous S and V
        }
    }

    // uses colorwheel
    public void AdjustHue(float newHue)
    {
        // convert hue to RGB

        float H, S, V;
        Color.RGBToHSV(paintColor, out H, out S, out V);
        // new color with new hue but existing S and V
        Color c = Color.HSVToRGB(newHue, S, V);

        SetHue(c);
        UpdateSV();

    }

    private Color AdjustSV () {
        
        float H, S, V;
        Color.RGBToHSV(paintColor, out H, out S, out V);
        Vector3 input = colorJoystick.GetInputDirection();

        S -= input.y * joystickSensitivity;
        V += input.x * joystickSensitivity;

        if (S <  0f) { S =  0f; };
        if (S >  1f) { S =  1f; };
        if (V <  0f) { V =  0f; };
        if (V >  1f) { V =  1f; };
       
        return Color.HSVToRGB(hue, S, V); // hue only changes when color slider is used
    }

    public void UpdateSV() {
        paintColor = AdjustSV(); // adjusts S and V without changing hue (hue shifts when it shouldn't otherwise)
        UpdateBrushColor();
    }

    private void UpdateBrushColor()
    {
        //Set the color of the brush, so user knows what color they are painting with
        // the outer, edge color -- adjust it to be darker
        brushColorMats[0].color = new Color(paintColor.r * colorDarken, paintColor.g * colorDarken, paintColor.b * colorDarken);
        brushColorMats[1].color = paintColor; // the inner color
        GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
        if (currentBrush)
        {
            currentAraTrail = currentBrush.GetComponent<AraTrail>();
            currentAraTrail.materials[0].color = paintColor;
            currentAraTrail.initialColor = paintColor;
        }
    }

    public void AdjustTargetDistance() {
        if (paintTarget) {
            paintTarget.transform.localPosition = new Vector3(0f, 0f, targetSlider.value);
        }
        GameObject currentPaintingPlane = GameObject.FindWithTag("CamPaintingPlane");
        if (currentPaintingPlane) {
            if (targetSlider)
            {
                currentPaintingPlane.transform.localPosition = new Vector3(0f, 0f, targetSlider.value);
                // roughly scale the grid up as it gets further away
                currentPaintingPlane.transform.localScale = new Vector3(0.5f * targetSlider.value, 0.5f * targetSlider.value, planeScale);
            }
        }
    }

    public void AdjustBrushSize() {
        if (paintTarget) {
            GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
            if (currentBrush)
            {
                currentBrush.GetComponent<AraTrail>().initialThickness = brushSize;
            }
        }
    }

    public void AdjustBrushThickness()
    {
        if (paintTarget)
        {
            GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
            if (currentBrush)
            {
                currentBrush.GetComponent<AraTrail>().thickness = brushSizeSlider.value;
            }
        }
    }

    // Add a mesh (or trailrender) painting brush to the paint target
    public void AddBrushToTarget()
    {
        // instantiate a brush and parent it to the paint target
        GameObject newBrush = Instantiate(paintBrushPrefab, new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0f,0f,0f)));
        newBrush.transform.parent = paintTarget.gameObject.transform; // attach the object that acts as a brush to the paintTarget
        newBrush.transform.localPosition = new Vector3(0f, 0f, 0f);
        //newBrush.GetComponent<TrailRenderer>().Clear(); // remove trail from 1st frame with odd, unwanted line (comes from Trail rendering before first point is established)
        newBrush.GetComponent<AraTrail>().initialColor = paintColor;
        newBrush.GetComponent<AraTrail>().initialThickness = brushSize;
    }


    public void RecreatePaintedStrokes() {
        paintstrokeParent.transform.position = new Vector3(0f, 0f, 1f);
        // The paint stroke info that was saved with the map should already have been put into paintStrokesList
        foreach (PaintStroke paintstroke in paintStrokesList)
        {
            // position the new paintbrush at the first point of the vertex list
            GameObject newBrush = Instantiate(paintBrushPrefab, paintstroke.verts[0], Quaternion.Euler(new Vector3(0f, 90f, 0f)));
            newBrush.transform.parent = paintstrokeParent.transform;
            AraTrail araTrail = newBrush.GetComponent<AraTrail>();
            araTrail.hasMoved = true; // on by default, controls whether a point is emitted
            araTrail.loading = true;
            araTrail.initialColor = paintstroke.color;
            araTrail.space = Space.Self; // allows the paintstroke to be moved with the parent's movement
            araTrail.trailstate = AraTrail.TrailState.Redrawing;
            StartCoroutine(PaintTrail(newBrush, paintstroke, araTrail));
        }

    }

    private IEnumerator PaintTrail(GameObject brush, PaintStroke paintstroke, AraTrail araTrail)
    {
        Vector3 paintstrokeOffset = new Vector3(0f,0f,0.5f);
        araTrail.hasMoved = true; // on by default, controls whether a point is emitted
        //brush.transform.parent = paintstrokeParent.transform;
        for (int i = 0; i < paintstroke.verts.Count; i++)
        {
            // can't set point color directly, so set the initialColor, which is then used to create the pointColor for the next point
            araTrail.initialColor = paintstroke.pointColors[i];
            // set the initialThickness, which is then used to create the size of the next point
            araTrail.initialThickness = paintstroke.pointSizes[i];
            brush.transform.position = paintstroke.verts[i] + paintstrokeOffset;

            yield return new WaitForSeconds(paintWait); // allow enough time for the previous mesh section to be generated
        }
        araTrail.loading = false;
        // Now that the PaintStroke has been saved, unparent it from the target so it's positioned in worldspace
        // TODO: refactor into its own method (repeated (almost) in RemoveBrushFromTarget())
        brush.tag = "PaintStroke";
        // Remove the reticle and brush, no longer needed
        foreach (Transform child in brush.transform)
        {
            Destroy(child.gameObject);
        }

    }

    public void RemoveBrushFromTarget() {
        // assuming there is only one paint brush as a time (but there may be multiple paintbrushes when reloading)
        GameObject brush = GameObject.FindWithTag("PaintBrush");
        if (brush)
        {
            // Add the verts of the trail renderer to PaintStrokeList
            AddPaintStrokeToList(brush);
            // Now that the PaintStroke has been saved, unparent it from the target so it's positioned in worldspace
            brush.tag = "PaintStroke";
            brush.transform.parent = null;
            // Remove the reticle and brush, no longer needed
            foreach (Transform child in brush.transform)
            {
                Destroy(child.gameObject);
            }
            paintOn = false;
        }
    }

    private void AddPaintStrokeToList (GameObject brush) {
        
        // 3rd party Ara Trails replaces Unity Trail Renderer
        List<Vector3> vertList = new List<Vector3>();
        List<Color> colorList = new List<Color>();
        List<float> sizeList = new List<float>();

            AraTrail araTrail = brush.GetComponent<AraTrail>();
            //araTrail.active = false; // not being actively drawn, so do not need to constantly update rounded end points
            araTrail.trailstate = AraTrail.TrailState.DrawnFlatEnd; // allow rounding points to be added. 
            araTrail.initialColor = paintColor;
            araTrail.initialThickness = brushSize;
            int numPosAra = araTrail.points.Count;

            for (int i = 0; i < numPosAra; i++)
            {
                vertList.Add(araTrail.points[i].position);
                colorList.Add(araTrail.points[i].color);
                sizeList.Add(araTrail.points[i].thickness);

                // alternately, could add the AraTrail points themselves to the Paintstroke, 
                // since they already hold the colors, plus other info such as discontinuous.
            }
        // Add the rounding endpoints to the list of points
        //for (int i = 0; i < araTrail.endPoints.Count; i++)
        //{
        //    vertList.Add(araTrail.endPoints[i].position);
        //    colorList.Add(Color.yellow); // test to see if endpoints are added //colorList.Add(araTrail.endPoints[i].color);
        //    sizeList.Add(araTrail.endPoints[i].thickness);
        //}


        // Only add the new PaintStrokes if it's newly created, not if loading from a saved map
        // TODO: Need a bool to prevent saving to list when strokes are being recreated?
        //if (!paintOnComponent.meshLoading)
        //{
            PaintStroke paintStroke = brush.AddComponent<PaintStroke>();
            paintStroke.color = paintColor;
            paintStroke.verts = vertList;
            paintStroke.pointColors = colorList;
            paintStroke.pointSizes = sizeList;
            paintStrokesList.Add(paintStroke);

     //   }
    }

    public void TogglePaint()
    {
        paintOn = !paintOn;

        // let user know that painting is on
        if (paintOn)
        {
            paintOnTouch = false;
            onoff.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
            paintButtonGroup.alpha = 1f;
            AddBrushToTarget();
        }
        else
        {
            RemoveBrushFromTarget();
            onoff.transform.localScale = new Vector3(1f, 1f, 1f); // return paint button to normal size
            paintButtonGroup.alpha = 0.4f;
  //          paintOnComponent.endPainting = true;
            paintOnTouch = true;
        }
    }

    public void RandomizeColor()
    {
        paintColor = UnityEngine.Random.ColorHSV(hueMin: 0f, hueMax: 1f, saturationMin: 0.8f, saturationMax: 1f, valueMin: 0.8f, valueMax: 1f);

        GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
        if (currentBrush)
        {
            currentBrush.GetComponent<AraTrail>().initialColor = paintColor;
        }
    }

    public void Reset()
    {
        // Discard current brush, if there is one actively painting
        DestroyBrush();
        // Discard all paintstrokes (not saving, unless user requested a save)
        DestroyAllPaintstrokes();
        // Reset all pertinent parameters
        paintOn = false;
        paintOnTouch = true;
    }

    private void DestroyBrush() {
        GameObject brush = GameObject.FindWithTag("PaintBrush");
        if (brush) {
            Destroy(brush);
        }
    }

    private void DestroyAllPaintstrokes() {
        GameObject[] paintstrokes = GameObject.FindGameObjectsWithTag("PaintStroke");
        foreach (GameObject paintstrokeObject in paintstrokes) {
            Destroy(paintstrokeObject);
        }
        paintStrokesList.Clear();
    }
}
