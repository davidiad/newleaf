using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.iOS;
using UnityEngine.UI;

public class PaintManager : MonoBehaviour
{

    // Get paint position from PlacenoteSampleView script
    public GameObject PSVGO;
    public Vector3 paintPosition;
    private PlacenoteSampleView PSV;
    public GameObject paintOnObject;

    public Button onoff;
    [SerializeField] private GameObject paintTarget;
    private Mesh mesh; // save particles in a mesh

    public ParticleSystem particleSystemTemplate;

    private bool newPaintVertices;
    private bool paintOn;
    private Color paintColor;
    private Vector3 previousPosition;

    public List<ParticleSystem> particleSystemList; // Stores all particle systems
    public List<Vector3> currVertices; // Stores current paint target positions to paint
    public ParticleSystem ps; // Stores current particle system
    public GameObject paintBrushPrefab;

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
        ps = Instantiate(particleSystemTemplate);
        currVertices = new List<Vector3>();
        paintColor = Color.blue;
        mesh = new Mesh();
        PSV = PSVGO.GetComponent<PlacenoteSampleView>();
        paintPosition = PSV.paintPosition;
        paintTarget = GameObject.FindWithTag("PaintTarget");
        paintOnObject = GameObject.FindWithTag("PaintOn");
    }

    // Update is called once per frame
    void Update()
    {
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

    // Add a mesh painting brush to the paint target
    private void AddBrush()
    {
        // instantiate a brush
        // parent it to the paint target
        GameObject newBrush = Instantiate(paintBrushPrefab, new Vector3(0f, 0f, 0f), Quaternion.Euler(new Vector3(0f,90f,0f)));
        newBrush.transform.parent = paintTarget.gameObject.transform;
        newBrush.transform.localPosition = new Vector3(0f, 0f, 0f);
    } 

    private void RemoveBrushFromTarget() {
        // assuming there is only one paint brush as a time
        GameObject brush = GameObject.FindWithTag("PaintBrush");
        if (brush)
        {
            brush.tag = "Mesh";
            brush.transform.parent = null;
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
            AddBrush();
        }
        else
        {
            RemoveBrushFromTarget();
            onoff.transform.localScale = new Vector3(1f, 1f, 1f);
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
