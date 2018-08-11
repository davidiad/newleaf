using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;
using Ara; // 3rd party Trail Renderer
using System;

// TODO:(?) Should this be a struct? (or Scriptable object?)
public class PaintStroke : MonoBehaviour
{
    //public int ID { get; set; }
    //public string SomethingWithText { get; set; }
    public List<Vector3> verts;// { get; set; }
    public List<Color> pointColors; // will hold colors of individual points
    public List<float> pointSizes; // will hold size of individual points
    public Color color;// { get; set; } // initial color of stroke (and default color if no point color)
}

public class PaintManager : MonoBehaviour
{
    public GameObject PSVGO;
    public Vector3 paintPosition;
    private LeavesView PSV;
    public GameObject paintOnObject;
    private PaintOn paintOnComponent;
    private GameObject targetSliderGO;
    private Slider targetSlider;
    private Slider paintSlider;
    private Slider brushSizeSlider;
    public float brushSize;

    public Button onoff;
    [SerializeField] private GameObject paintTarget;

    public ParticleSystem particleSystemTemplate;

    private bool newPaintVertices;
    private bool paintOn;
    private Color paintColor;
    private Material[] brushColorMats;
    private Vector3 previousPosition;
    public float strokeThickness; // multiplier, sets overall thickness of trail
    private float colorDarken = 0.65f; // amount to darken the outer rings of the cursor
    public ColorJoystick colorJoystick;
    private Vector3 colorInput;
    private float hue;
    public float joystickSensitivity = 0.0125f;


    public List<PaintStroke> paintStrokesList;
    public List<ParticleSystem> particleSystemList; // Stores all particle systems
    public List<Vector3> currVertices; // Stores current paint target positions to paint

    public ParticleSystem ps; // Stores current particle system
    public GameObject paintBrushPrefab;
    private GameObject paintBrush;
    private CanvasGroup paintButtonGroup;
    [SerializeField] Camera mainCam;

    public bool paintOnTouch;
    public bool ARPlanePainting;

    [SerializeField] private float paintWait; // time to wait before adding next vertex when painting trail

    //void Awake()
    //{
    //    Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
    //}

    void Start()
    {
        paintWait = 0.01f; // factor to control speed of re-drawing saved paintstrokes
        brushSize = 0.005f; // in meters
        strokeThickness = 1f;
        paintOn = false;
        paintOnTouch = true;
        newPaintVertices = false;
        particleSystemList = new List<ParticleSystem>();
        paintStrokesList = new List<PaintStroke>();
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
        paintColor = Color.blue;
        brushColorMats = GameObject.FindWithTag("BrushColor").GetComponent<Renderer>().materials;
        PSV = PSVGO.GetComponent<LeavesView>();
        paintPosition = PSV.paintPosition;
        paintTarget = GameObject.FindWithTag("PaintTarget");
        paintSlider = GameObject.FindWithTag("PaintSlider").GetComponent<Slider>();
        brushSizeSlider = GameObject.FindWithTag("SizeSlider").GetComponent<Slider>();
        paintOnObject = GameObject.FindWithTag("PaintOn");
        paintOnComponent = paintOnObject.GetComponent<PaintOn>();
        targetSliderGO = GameObject.FindWithTag("TargetSlider");
        targetSlider = targetSliderGO.GetComponent<Slider>();
        AdjustTargetDistance();
        AdjustPaintColor(); // set the color to what the color slider is set to
        paintButtonGroup = onoff.GetComponent<CanvasGroup>();
        paintButtonGroup.alpha = 0.4f;
        //colorJoystick = GameObject.FindWithTag("ColorJoystick").GetComponent<ColorJoystick>();
        SetHue(paintColor);

    }
 
    void Update()
    {
        colorInput = colorJoystick.GetInputDirection();
        if (colorInput != Vector3.zero)
        {
            UpdateSV();
        }

        currVertices = paintOnComponent.currentVertices; //TODO: update only when needed, not every frame

        bool endPainting = paintOnComponent.endPainting;

        if (endPainting)
        {
            /*
            // Get the vertices of the trail renderer(s)
            Vector3[] positions = new Vector3[1000]; // assuming there'll never be > 1000
            paintBrush = GameObject.FindWithTag("PaintBrush");
            if (!paintBrush){
                paintBrush = GameObject.FindGameObjectsWithTag("Mesh")[0];
            }
            // TrailRenderer.GetPositions adds its positions to an existing arrays, and returns the # of vertices
            int numPos = paintBrush.GetComponent<TrailRenderer>().GetPositions(positions);
            List<Vector3> vertList = new List<Vector3>();
            for (int i = 0; i < numPos; i++) {
                vertList.Add(positions[i]);
            }

            //***New Way, can hold >1 paint mesh/strokes***
            // Only add the new PaintStrokes if it's newly created, not if loading from a saved map
            if (!paintOnComponent.meshLoading) 
            {
                PaintStrokes paintStrokes = new PaintStrokes();
                paintStrokes.verts = vertList;
                paintStrokesList.Add(paintStrokes);
            }

            paintOnComponent.currentVertices = vertList; // old way, still working

            paintBrush.transform.parent = null;
            paintBrush.tag = "Mesh";
            */
            //paintOnComponent.endPainting = false;
            //paintOnComponent.meshLoading = false;
            //Destroy(this.gameObject.GetComponent(ExtrudedMeshTrail));
        }
    }

    // to keep hue constant when setting S and V
    private void SetHue(Color color) {
        float H, S, V;
        Color.RGBToHSV(color, out H, out S, out V);
        hue = H;
    }


    public void AdjustPaintColor() {
        if (paintSlider)
        {
            Gradient paintGradient = paintSlider.GetComponent<PaintGradient>().gradient;
            Color tempColor = paintGradient.Evaluate(paintSlider.value);
            SetHue(tempColor); // get the hue from the gradient slider
            UpdateSV(); // Update paintColor with new hue, and previous S and V
        }
    }

    private Color AdjustSV () {
        
        float H, S, V;
        Color.RGBToHSV(paintColor, out H, out S, out V);
        Vector3 input = colorJoystick.GetInputDirection();
        //if (Mathf.Abs(S) < 1)
        //{
            S -= input.y * joystickSensitivity;
        //}
        //if (Mathf.Abs(V) < 1)
        //{
            V += input.x * joystickSensitivity;
        //}
        if (S <  0f) { S =  0f; };
        if (S >  1f) { S =  1f; };
        if (V <  0f) { V =  0f; };
        if (V >  1f) { V =  1f; };
        Debug.Log("Hue: " + hue + "    " + "S: " +  S + "    " + "V: " + V);
       
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
            currentBrush.GetComponent<AraTrail>().initialColor = paintColor;
        }
    }

    public void AdjustTargetDistance() {
        if (paintTarget)
        {
            paintTarget.transform.localPosition = new Vector3(0f, 0f, targetSlider.value);
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
        paintOnComponent.meshLoading = true;
        // The paint stroke info that was saved with the map should already have been put into paintStrokesList
        foreach (PaintStroke paintstroke in paintStrokesList)
        {
            //if (paintstroke.verts.Count > 2) // no point in drawing a single vert
           // {
                // position the new paintbrush at the first point of the vertex list
                GameObject newBrush = Instantiate(paintBrushPrefab, paintstroke.verts[0], Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                AraTrail araTrail = newBrush.GetComponent<AraTrail>();
                araTrail.initialColor = paintstroke.color;
                StartCoroutine(PaintTrail(newBrush, paintstroke, araTrail));
            //}
        }
        paintOnComponent.meshLoading = false;
        paintOnComponent.endPainting = true; // flag to destroy mesh extrusion component
    }

    private IEnumerator PaintTrail(GameObject brush, PaintStroke paintstroke, AraTrail araTrail)
    {
        for (int i = 0; i < paintstroke.verts.Count; i++)
        {
            // can't set point color directly, so set the initialColor, which is then used to create the pointColor for the next point
            araTrail.initialColor = paintstroke.pointColors[i];
            // set the initialThickness, which is then used to create the size of the next point
            araTrail.initialThickness = paintstroke.pointSizes[i]; 
            brush.transform.position = paintstroke.verts[i];

            yield return new WaitForSeconds(paintWait); // allow enough time for the previous mesh section to be generated
        }
        //// Add the verts of the trail renderer to PaintStrokeList
        //AddPaintStrokeToList(brush);
        // Now that the PaintStroke has been saved, unparent it from the target so it's positioned in worldspace
        brush.tag = "PaintStroke";
        brush.transform.parent = null;
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
            paintOnComponent.paintOn = false;
        }
    }

    private void AddPaintStrokeToList (GameObject brush) {
        
        // 3rd party Ara Trails replaces Unity Trail Renderer
        List<Vector3> vertList = new List<Vector3>();
        List<Color> colorList = new List<Color>();
        List<float> sizeList = new List<float>();
        // TrailRenderer.GetPositions adds its positions to an existing arrays, and returns the # of vertices
        // Get the vertices of the trail renderer(s)
        //Vector3[] positions = new Vector3[1000]; // assuming there'll never be > 1000
        // Note: // Trail Renderer may be turned off in favor of Ara Trail Renderer
        //int numPos = brush.GetComponent<TrailRenderer>().GetPositions(positions);
        //if (numPos > 0) 
        //{
        //    for (int i = 0; i < numPos; i++)
        //    {
        //        vertList.Add(positions[i]);
        //    }
        //} else {
            // Ara Trail version

            AraTrail araTrail = brush.GetComponent<AraTrail>();
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
        //}

        // Only add the new PaintStrokes if it's newly created, not if loading from a saved map
        if (!paintOnComponent.meshLoading)
        {
            PaintStroke paintStroke = brush.AddComponent<PaintStroke>();
            paintStroke.color = paintColor;
            paintStroke.verts = vertList;
            paintStroke.pointColors = colorList;
            paintStroke.pointSizes = sizeList;
            paintStrokesList.Add(paintStroke);
        }
    }

    public void TogglePaint()
    {
        
        paintOn = !paintOn;
        //paintOnObject.PaintOn.paintOn = paintOn;// the state to an object from which other scripts can access
        paintOnObject.GetComponent<PaintOn>().paintOn = paintOn; 

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
            paintOnComponent.endPainting = true;
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
        paintOnComponent.paintOn = false;
        paintOnComponent.endPainting = true;
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

    private void Paint()
    {
        paintPosition = PSV.paintPosition;
        if (Vector3.Distance(paintPosition, previousPosition) > 0.025f)
        {
            if (paintOn) currVertices.Add(paintPosition);
            previousPosition = paintPosition;
            newPaintVertices = true;

        }
    }
}
