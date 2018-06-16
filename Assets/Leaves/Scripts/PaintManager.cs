using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;


public class PaintStroke
{
    //public int ID { get; set; }
    //public string SomethingWithText { get; set; }
    public List<Vector3> verts { get; set; }
    public Color color { get; set; }

}

public class PaintManager : MonoBehaviour
{
    //public class PaintingList : List<PaintStrokes> { }
    // Get paint position from PlacenoteSampleView script
    public GameObject PSVGO;
    public Vector3 paintPosition;
    private LeavesView PSV;
    public GameObject paintOnObject;
    private PaintOn paintOnComponent;
    private GameObject targetSliderGO;
    private Slider targetSlider;

    public Button onoff;
    [SerializeField] private GameObject paintTarget;
    private Mesh mesh; // save particles in a mesh

    public ParticleSystem particleSystemTemplate;

    private bool newPaintVertices;
    private bool paintOn;
    private Color paintColor;
    private Vector3 previousPosition;

    public List<PaintStroke> paintStrokesList;
    public List<ParticleSystem> particleSystemList; // Stores all particle systems
    public List<Vector3> currVertices; // Stores current paint target positions to paint

    public ParticleSystem ps; // Stores current particle system
    public GameObject paintBrushPrefab;
    private GameObject paintBrush;

    [SerializeField] Camera mainCam;

    void OnEnable()
    {
        //UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
    }

    void OnDestroy()
    {
        //UnityARSessionNativeInterface.ARFrameUpdatedEvent -= ARFrameUpdated;
    }

    // Use this for initialization
    void Start()
    {
       
        paintOn = false;
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
        paintOnObject = GameObject.FindWithTag("PaintOn");
        paintOnComponent = paintOnObject.GetComponent<PaintOn>();
        targetSliderGO = GameObject.FindWithTag("TargetSlider");
        targetSlider = targetSliderGO.GetComponent<Slider>();
    }

    // Update is called once per frame
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

    public void AdjustTargetDistance() {
        if (paintTarget)
        {
            paintTarget.transform.localPosition = new Vector3(0f, 0f, targetSlider.value);
        }
    }

    // Add a mesh painting brush to the paint target
    private void AddBrushToTarget()
    {
        // instantiate a brush
        // parent it to the paint target
        GameObject newBrush = Instantiate(paintBrushPrefab, new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0f,0f,0f)));
        newBrush.transform.parent = paintTarget.gameObject.transform; // attach the object that acts as a brush to the paintTarget
        newBrush.transform.localPosition = new Vector3(0f, 0f, 0f);
        newBrush.GetComponent<TrailRenderer>().Clear(); // remove trail from 1st frame with odd, unwanted line (comes from Trail rendering before first point is established)
    } 

    public void RecreatePaintedStrokes() {
        paintOnComponent.meshLoading = true;
        // position the paintbrush at the first point of the vertex list
        //TODO: Use the entire paint stroke list
        if (currVertices.Count > 0)
        {
            GameObject newBrush = Instantiate(paintBrushPrefab, currVertices[0], Quaternion.Euler(new Vector3(0f, 90f, 0f)));
            StartCoroutine(PaintMesh(newBrush));
        }
    }

    private IEnumerator PaintMesh(GameObject brush) {
        Debug.Log("In PaintMesh coroutine");
        for (int i = 1; i < currVertices.Count; i++) {
            Debug.Log("i: " + i);
            Debug.Log("currVertices[i]: " + currVertices[i]);
            brush.transform.position = currVertices[i];
            yield return new WaitForSeconds(0.01f); // allow enough time for the previous mesh section to be generated
        }
        //TODO: Loop thru all groups of verts, not just 1
        paintOnComponent.meshLoading = false;
        paintOnComponent.endPainting = true; // flag to destroy mesh extrusion component
    }

    private void RemoveBrushFromTarget() {
        // assuming there is only one paint brush as a time
        GameObject brush = GameObject.FindWithTag("PaintBrush");
        if (brush)
        {
            // Add the verts of the trail renderer to PaintStrokeList
            AddPaintStrokeToList();
            // Now that the PaintStroke has been saved, unparent it from the target so it's positioned in worldspace
            brush.tag = "PaintStroke";
            brush.transform.parent = null;
        }
    }

    private void AddPaintStrokeToList () {
        // Get the vertices of the trail renderer(s)
        Vector3[] positions = new Vector3[1000]; // assuming there'll never be > 1000
        paintBrush = GameObject.FindWithTag("PaintBrush"); // Will be a different paintBrush each time, so set here, not in Start

        // TrailRenderer.GetPositions adds its positions to an existing arrays, and returns the # of vertices
        int numPos = paintBrush.GetComponent<TrailRenderer>().GetPositions(positions);
        List<Vector3> vertList = new List<Vector3>();
        for (int i = 0; i < numPos; i++)
        {
            vertList.Add(positions[i]);
        }

        // Only add the new PaintStrokes if it's newly created, not if loading from a saved map
        if (!paintOnComponent.meshLoading)
        {
            PaintStroke paintStroke = new PaintStroke();
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
            onoff.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
            AddBrushToTarget();
        }
        else
        {
            RemoveBrushFromTarget();
            onoff.transform.localScale = new Vector3(1f, 1f, 1f);
            paintOnComponent.endPainting = true;
        }
    }

    public void RandomizeColor()
    {
        if (ps.particleCount > 0)
        {
            SaveParticleSystem();
        }
        paintColor = Random.ColorHSV();
    }

    public void Reset()
    {
        foreach (ParticleSystem p in particleSystemList)
        {
            Destroy(p);
        }
        particleSystemList = new List<ParticleSystem>();

        Destroy(ps);
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
    }

    private void SaveParticleSystem()
    {
        particleSystemList.Add(ps);
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
    }

    public Mesh GetMesh()
    {
        // get all the particles, and save them in a mesh
        //foreach (ParticleSystem partSys in particleSystemList)
        //{
        /*
        ParticleSystem.Particle[] myParticles = (ParticleSystem)GetComponent("ParticleSystem");
        partSys.GetParticles(myParticles);
        foreach (Particle particle in myParticles)
        {

        }

        ParticleSystem.Particle[] currentParticles = new ParticleSystem.Particle[partSys.particleCount]; 
        partSys.GetParticles(currentParticles);
        foreach (Particle particle in currentParticles) 
        {

        }
        Vector3 [] verts = partSys.GetParticles()

    }
    */
        return mesh;
    }

    private void Paint()
    {
       // paintPosition = paintTarget.transform.position;
        paintPosition = PSV.paintPosition;
        //Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, Screen.height / 2, Camera.main.nearClipPlane)) + Camera.main.transform.forward * 2.0f;
        //Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
        if (Vector3.Distance(paintPosition, previousPosition) > 0.025f)
        {
            if (paintOn) currVertices.Add(paintPosition);
            previousPosition = paintPosition;
            newPaintVertices = true;

        }
    }
    // Quaternion rot = Camera.main.transform.rotation;
    /*
    private void ARFrameUpdated(UnityARCamera arCamera)
    {

        Vector3 paintPosition = (Camera.main.transform.forward * 0.2f) + GetCameraPosition(arCamera);
        if (Vector3.Distance(paintPosition, previousPosition) > 0.025f)
        {
            if (paintingOn) currVertices.Add(paintPosition);
            previousPosition = paintPosition;
            newPaintVertices = true;
            Debug.Log("arCam Position: " + paintPosition);
        } else {
            Debug.Log("arCam Position, Painting off: " + paintPosition);
        }
    }
    */

    private Vector3 GetCameraPosition(UnityARCamera cam)
    {
        Matrix4x4 matrix = new Matrix4x4();
        matrix.SetColumn(3, cam.worldTransform.column3);
        return UnityARMatrixOps.GetPosition(matrix);
    }
}
