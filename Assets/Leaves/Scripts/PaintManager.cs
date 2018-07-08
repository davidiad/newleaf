using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;
using Ara; // 3rd party Trail Renderer

// TODO:(?) Should this be a struct? (or Scriptable object?)
public class PaintStroke : MonoBehaviour
{
    //public int ID { get; set; }
    //public string SomethingWithText { get; set; }
    public List<Vector3> verts;// { get; set; }
    public List<Color> pointColors; // will hold colors of individual points
    // public List<float> pointSizes // will hold size of individual points
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
    private Mesh mesh; // save particles in a mesh

    public ParticleSystem particleSystemTemplate;

    private bool newPaintVertices;
    private bool paintOn;
    private Color paintColor;
    private Material brushColorMat;
    private Vector3 previousPosition;
    public float strokeThickness; // multiplier, sets overall thickness of trail

    public List<PaintStroke> paintStrokesList;
    public List<ParticleSystem> particleSystemList; // Stores all particle systems
    public List<Vector3> currVertices; // Stores current paint target positions to paint

    public ParticleSystem ps; // Stores current particle system
    public GameObject paintBrushPrefab;
    private GameObject paintBrush;
    private CanvasGroup paintButtonGroup;
    [SerializeField] Camera mainCam;

    public bool paintOnTouch;

    void OnEnable()
    {
        //UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
    }

    void OnDestroy()
    {
        //UnityARSessionNativeInterface.ARFrameUpdatedEvent -= ARFrameUpdated;
    }

    void Start()
    {
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
        mesh = new Mesh();
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
        brushColorMat = GameObject.FindWithTag("BrushColor").GetComponent<Renderer>().sharedMaterial;
        brushColorMat.color = paintColor;
    }
 
    void Update()
    {
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




        /*
        if (paintOn)
        {
            Paint();
        }

        if (paintOn && newPaintVertices)
        {
            if (currVertices.Count > 0)
            {
                ParticleSystem.Particle[] particles = new ParticleSystem.Particle[currVertices.Count];
                int index = 0;
                foreach (Vector3 vtx in currVertices)
                {
                    particles[index].position = vtx;
                    particles[index].color = paintColor;
                    particles[index].size = 0.05f;
                    index++;
                }
                ps.SetParticles(particles, currVertices.Count);
                newPaintVertices = false;
            }
        }

*/
    }

    public void AdjustPaintColor() {
        if (paintSlider) {
            Gradient paintGradient = paintSlider.GetComponent<PaintGradient>().gradient;
            paintColor = paintGradient.Evaluate(paintSlider.value);
            GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
            if (currentBrush)
            {
                currentBrush.GetComponent<AraTrail>().initialColor = paintColor;
            }
            brushColorMat = GameObject.FindWithTag("BrushColor").GetComponent<Renderer>().sharedMaterial;
            brushColorMat.color = paintColor; // Set the color of the brush, so user knows what color they are painting with
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
        // instantiate a brush
        // parent it to the paint target
        GameObject newBrush = Instantiate(paintBrushPrefab, new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0f,0f,0f)));
        newBrush.transform.parent = paintTarget.gameObject.transform; // attach the object that acts as a brush to the paintTarget
        newBrush.transform.localPosition = new Vector3(0f, 0f, 0f);
        newBrush.GetComponent<TrailRenderer>().Clear(); // remove trail from 1st frame with odd, unwanted line (comes from Trail rendering before first point is established)

        newBrush.GetComponent<AraTrail>().initialColor = paintColor;
    } 

    public void RecreatePaintedStrokes() {
        paintOnComponent.meshLoading = true;
        // The paint stroke info that was saved with the map should already have been put into paintStrokesList
        foreach (PaintStroke paintstroke in paintStrokesList)
        {
            if (paintstroke.verts.Count > 2) // no point in drawing a single vert
            {
                // position the new paintbrush at the first point of the vertex list
                GameObject newBrush = Instantiate(paintBrushPrefab, paintstroke.verts[0], Quaternion.Euler(new Vector3(0f, 90f, 0f)));
                newBrush.GetComponent<AraTrail>().initialColor = paintstroke.color;
                StartCoroutine(PaintTrail(newBrush, paintstroke));
            }
        }
        paintOnComponent.meshLoading = false;
        paintOnComponent.endPainting = true; // flag to destroy mesh extrusion component
        // previous way, does only the first one
        //if (currVertices.Count > 0)
        //{
        //    GameObject newBrush = Instantiate(paintBrushPrefab, currVertices[0], Quaternion.Euler(new Vector3(0f, 90f, 0f)));
        //    StartCoroutine(PaintMesh(newBrush));
        //}
    }

    private IEnumerator PaintTrail(GameObject brush, PaintStroke paintstroke)
    {
        for (int i = 1; i < paintstroke.verts.Count; i++)
        {
            brush.transform.position = paintstroke.verts[i];
            yield return new WaitForSeconds(0.01f); // allow enough time for the previous mesh section to be generated
        }
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
    }

    //private IEnumerator PaintMesh(GameObject brush) {
    //    //Debug.Log("In PaintMesh coroutine");
    //    for (int i = 1; i < currVertices.Count; i++) {
    //        //Debug.Log("i: " + i);
    //        //Debug.Log("currVertices[i]: " + currVertices[i]);
    //        brush.transform.position = currVertices[i];
    //        yield return new WaitForSeconds(0.01f); // allow enough time for the previous mesh section to be generated
    //    }
    //    //TODO: Loop thru all groups of verts, not just 1
    //    paintOnComponent.meshLoading = false;
    //    paintOnComponent.endPainting = true; // flag to destroy mesh extrusion component
    //}



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
        List<Vector3> vertList = new List<Vector3>();
        List<Color> colorList = new List<Color>();
        // TrailRenderer.GetPositions adds its positions to an existing arrays, and returns the # of vertices
        // Get the vertices of the trail renderer(s)
        Vector3[] positions = new Vector3[1000]; // assuming there'll never be > 1000
        // Note: // Trail Renderer may be turned off in favor of Ara Trail Renderer
        int numPos = brush.GetComponent<TrailRenderer>().GetPositions(positions);
        if (numPos > 0) 
        {
            for (int i = 0; i < numPos; i++)
            {
                vertList.Add(positions[i]);
            }
        } else {
            // Ara Trail version
            AraTrail araTrail = brush.GetComponent<AraTrail>();
            araTrail.initialColor = paintColor;
            araTrail.initialThickness = brushSize;
            int numPosAra = araTrail.points.Count;

            for (int i = 0; i < numPosAra; i++)
            {
                vertList.Add(araTrail.points[i].position);
                colorList.Add(araTrail.points[i].color); 
                // alternately, could add the AraTrail points themselves to the Painstroke, 
                // since they already hold the colors, plus other info such as discontinuous.
            }
        }



        // Only add the new PaintStrokes if it's newly created, not if loading from a saved map
        if (!paintOnComponent.meshLoading)
        {
            PaintStroke paintStroke = brush.AddComponent<PaintStroke>();
            paintStroke.color = paintColor;
            paintStroke.verts = vertList;
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
        //if (ps.particleCount > 0)
        //{
        //    SaveParticleSystem();
        //}

        paintColor = Random.ColorHSV(hueMin: 0f, hueMax: 1f, saturationMin: 0.8f, saturationMax: 1f, valueMin: 0.8f, valueMax: 1f);

        GameObject currentBrush = GameObject.FindWithTag("PaintBrush");
        if (currentBrush)
        {
            currentBrush.GetComponent<AraTrail>().initialColor = paintColor;
        }
    }

    public void Reset()
    {
        /* for reference, previous system that used particles
        foreach (ParticleSystem p in particleSystemList)
        {
            Destroy(p);
        }
        particleSystemList = new List<ParticleSystem>();

        Destroy(ps);
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
        */

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

    private void SaveParticleSystem()
    {
        particleSystemList.Add(ps);
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
    }


    private void Paint()
    {
       // paintPosition = paintTarget.transform.position;
        paintPosition = PSV.paintPosition;
        if (Vector3.Distance(paintPosition, previousPosition) > 0.025f)
        {
            if (paintOn) currVertices.Add(paintPosition);
            previousPosition = paintPosition;
            newPaintVertices = true;

        }
    }

    private Vector3 GetCameraPosition(UnityARCamera cam)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetColumn(3, cam.worldTransform.column3);
        return UnityARMatrixOps.GetPosition(matrix);
    }
}
