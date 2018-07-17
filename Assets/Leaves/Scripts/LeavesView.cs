using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

[System.Serializable]
public class ShapeInfo
{
    public float px;
    public float py;
    public float pz;
    public float qx;
    public float qy;
    public float qz;
    public float qw;
    public int shapeType;
}

[System.Serializable]
// analogous to SV3List, but could add other attributes, such as color
public class PaintStrokeInfo
{
    public SerializableVector3[] verts;
    public SerializableVector4 initialColor; // initial color of stroke. Color implicitly converts to Vector4.
}

[System.Serializable]
public class PaintStrokeList
{
    public PaintStrokeInfo[] strokes;
}


[System.Serializable]
public class SV3List
{
    public SerializableVector3[] sv3s;
}


[System.Serializable]
public class ShapeList
{
    public ShapeInfo[] shapes;
}


public class LeavesView : MonoBehaviour, PlacenoteListener
{
    // Getting refs to buttons in the UI
    [SerializeField] GameObject mMapSelectedPanel;
    [SerializeField] GameObject mInitButtonPanel;
    [SerializeField] GameObject mMappingButtonPanel;
    [SerializeField] GameObject mMapListPanel;
    [SerializeField] GameObject mExitButton;
    [SerializeField] GameObject mListElement;
    [SerializeField] RectTransform mListContentParent;
    [SerializeField] ToggleGroup mToggleGroup;
    [SerializeField] GameObject mPlaneDetectionToggle;
    [SerializeField] Text mLabelText;
    [SerializeField] Material mShapeMaterial;
    [SerializeField] PlacenoteARGeneratePlane mPNPlaneManager;
    [SerializeField] Text uploadText;
    GameObject mapButton;

    public Vector3 paintPosition;

    private UnityARSessionNativeInterface mSession;
    private bool mFrameUpdated = false;
    private UnityARImageFrameData mImage = null;
    private UnityARCamera mARCamera;
    private bool mARKitInit = false;
    private List<ShapeInfo> shapeInfoList = new List<ShapeInfo>();
    private List<GameObject> shapeObjList = new List<GameObject>();
    private List<SerializableVector3> v3list = new List<SerializableVector3>();
    private List<PaintStrokeInfo> paintStrokeInfoList = new List<PaintStrokeInfo>();
    private List<PaintStroke> paintStrokeObjList = new List<PaintStroke>();

    private PaintManager paintManager;

    private LibPlacenote.MapInfo mSelectedMapInfo;

    private bool hasLocalized; // flag to prevent continually reloading the metadata when position is lost and regained
    private bool mappingStarted;
    private string currentMapId;

    private Coroutine pulseMapButton;

    private string mSelectedMapId
    {
        get
        {
            return 
            mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
        }
    }

    private BoxCollider mBoxColliderDummy;
    private SphereCollider mSphereColliderDummy;
    private CapsuleCollider mCapColliderDummy;

    bool ARPlanePaintingStatus; 

    // Use this for initialization
    void Start()
    {
        currentMapId = "";
        mappingStarted = false;
        hasLocalized = false;
        Input.location.Start();

        mMapListPanel.SetActive(false);

        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
        StartARKit();
        FeaturesVisualizer.EnablePointcloud();
        LibPlacenote.Instance.RegisterListener(this);


        GameObject pmgo = GameObject.FindWithTag("PaintManager");
        paintManager = pmgo.GetComponent<PaintManager>();
        ARPlanePaintingStatus = mPlaneDetectionToggle.GetComponent<Toggle>().isOn;
        paintManager.ARPlanePainting = ARPlanePaintingStatus;
        paintManager.paintOnTouch = !ARPlanePaintingStatus; // TODO: make an enum to replace multiple bools
        mapButton = GameObject.FindWithTag("MapButton");

    }


    private void ARFrameUpdated(UnityARCamera camera)
    {
        mFrameUpdated = true;
        mARCamera = camera;
    }


    private void InitARFrameBuffer()
    {
        mImage = new UnityARImageFrameData();

        int yBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yHeight;
        mImage.y.data = Marshal.AllocHGlobal(yBufSize);
        mImage.y.width = (ulong)mARCamera.videoParams.yWidth;
        mImage.y.height = (ulong)mARCamera.videoParams.yHeight;
        mImage.y.stride = (ulong)mARCamera.videoParams.yWidth;

        // This does assume the YUV_NV21 format
        int vuBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yWidth / 2;
        mImage.vu.data = Marshal.AllocHGlobal(vuBufSize);
        mImage.vu.width = (ulong)mARCamera.videoParams.yWidth / 2;
        mImage.vu.height = (ulong)mARCamera.videoParams.yHeight / 2;
        mImage.vu.stride = (ulong)mARCamera.videoParams.yWidth;

        mSession.SetCapturePixelData(true, mImage.y.data, mImage.vu.data);
    }

    void Update()
    {
        if (!mappingStarted) // start mapping automatically
        {
            OnNewMapClick();
        }
        if (mFrameUpdated)
        {
            mFrameUpdated = false;
            if (mImage == null)
            {
                InitARFrameBuffer();
            }

            if (mARCamera.trackingState == ARTrackingState.ARTrackingStateNotAvailable)
            {
                // ARKit pose is not yet initialized
                return;
            }
            else if (!mARKitInit)
            {
                mARKitInit = true;
                mLabelText.text = "ARKit Initialized";
                if (!LibPlacenote.Instance.Initialized())
                {
                    Debug.Log("initialized Mapping");


                }

            }

            Matrix4x4 matrix = mSession.GetCameraPose();

            Vector3 arkitPosition = PNUtility.MatrixOps.GetPosition(matrix);
            Quaternion arkitQuat = PNUtility.MatrixOps.GetRotation(matrix);

            LibPlacenote.Instance.SendARFrame(mImage, arkitPosition, arkitQuat,
                                              mARCamera.videoParams.screenOrientation);

        }

        paintPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
        if (mappingStarted)
        {
            pulseMapButton = StartCoroutine(PulseColor(mapButton.GetComponent<Image>()));
        }
    }

    //private void LateUpdate()
    //{
    //    if (mARKitInit)
    //    {
    //        mLabelText.text = "Auto start Mapping";
    //        // automatically start mapping for Placenote map
    //        OnNewMapClick();
    //    }
    //}

    private void ActivateMapButton(bool mappingOn) {
        Image imageComponent = mapButton.GetComponent<Image>();
        imageComponent.fillCenter = mappingOn;
        if (mappingOn)
        {
            mapButton.GetComponent<CanvasGroup>().alpha = 1.0f;
            imageComponent.color = Color.yellow;
            //pulseMapButton = StartCoroutine(PulseColor(imageComponent));
        } else {
            mapButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            StopCoroutine(pulseMapButton);
            imageComponent.color = Color.white;
        }
    }

    private IEnumerator PulseColor(Image img)
    {
        float alpha = (Mathf.Sin(Time.time * 2f) + 1.9f) * 0.33f;
        img.color = new Color(0.5f, 0.5f, 0f, alpha);

        yield return null;
    }

    public void OnListMapClick()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            ToastManager.ShowToast("SDK not yet initialized", 2f);
            return;
        }

        foreach (Transform t in mListContentParent.transform)
        {
            // TODO: check if this destroy command is a cause of the slow loading of the list
            Destroy(t.gameObject);
        }

        mMapListPanel.SetActive(true);
        mInitButtonPanel.SetActive(false);
        LibPlacenote.Instance.ListMaps((mapList) =>
        {
            // render the map list!
            foreach (LibPlacenote.MapInfo mapId in mapList)
            {
                if (mapId.userData != null)
                {
                    //Debug.Log(mapId.userData.ToString(Formatting.None));
                }
                AddMapToList(mapId);
            }
        });
    }


    public void OnCancelClick()
    {
        mMapSelectedPanel.SetActive(false);
        mMapListPanel.SetActive(false);
        mInitButtonPanel.SetActive(true);
    }


    public void OnExitClick()
    {
        paintManager.Reset();
        mInitButtonPanel.SetActive(true);
        mExitButton.SetActive(false);
        mPlaneDetectionToggle.SetActive(false);

        //clear all existing planes
        mPNPlaneManager.ClearPlanes();
        mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;

        LibPlacenote.Instance.StopSession();
        mappingStarted = false; // allow a new mapping session to begin auto. in Update
        ActivateMapButton(false); // Should change to true in NewMapClicked
        hasLocalized = false;

    }


    void AddMapToList(LibPlacenote.MapInfo mapInfo)
    {
        GameObject newElement = Instantiate(mListElement) as GameObject;
        MapInfoElement listElement = newElement.GetComponent<MapInfoElement>();
        listElement.Initialize(mapInfo, mToggleGroup, mListContentParent, (value) =>
        {
            OnMapSelected(mapInfo);
        });
    }


    void OnMapSelected(LibPlacenote.MapInfo mapInfo)
    {
        mSelectedMapInfo = mapInfo;
        mMapSelectedPanel.SetActive(true);
    }


    public void OnLoadMapClicked()
    {
        ConfigureSession(false);
        // Since a session starts running after app launch automatically,
        // ensure that if a session is already running, it is stopped
        LibPlacenote.Instance.StopSession();
        ActivateMapButton(false);
        paintManager.Reset();
        hasLocalized = false; // reset flag that limits localization to just once
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            ToastManager.ShowToast("SDK not yet initialized", 2f);
            return;
        }
        hasLocalized = false;
        mLabelText.text = "Loading Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.LoadMap(mSelectedMapId,
            (completed, faulted, percentage) =>
            {
                if (completed)
                {
                    mMapSelectedPanel.SetActive(false);
                    mMapListPanel.SetActive(false);
                    mInitButtonPanel.SetActive(false);
                    mExitButton.SetActive(true);
                    mPlaneDetectionToggle.SetActive(true);

                    LibPlacenote.Instance.StartSession();
                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
                    currentMapId = mSelectedMapId;
                    //Debug.Log("VERSION?: ");
                    //Debug.Log(mSelectedMapInfo.userData["version"]["a"].ToObject<float>());
                }
                else if (faulted)
                {
                    mLabelText.text = "Failed to load ID: " + mSelectedMapId;
                }
            }
        );
    }

    public void OnDeleteMapClicked()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            ToastManager.ShowToast("SDK not yet initialized", 2f);
            return;
        }

        mLabelText.text = "Deleting Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.DeleteMap(mSelectedMapId, (deleted, errMsg) =>
        {
            if (deleted)
            {
                mMapSelectedPanel.SetActive(false);
                mLabelText.text = "Deleted ID: " + mSelectedMapId;
                OnListMapClick();
            }
            else
            {
                mLabelText.text = "Failed to delete ID: " + mSelectedMapId;
            }
        });
    }


    public void OnNewMapClick() {
        ConfigureSession(false);       

        if (LibPlacenote.Instance.Initialized())
        {
            if (!mappingStarted)
            {
                GameObject.FindWithTag("MapButton").GetComponent<CanvasGroup>().alpha = 1.0f;
                mMappingButtonPanel.SetActive(true);
                mPlaneDetectionToggle.SetActive(true);
                Debug.Log("Started Session");
                mappingStarted = true;
                ActivateMapButton(mappingStarted);
                LibPlacenote.Instance.StartSession();
            }
        }





        /*
        if (LibPlacenote.Instance.Initialized())
        {
            //mInitButtonPanel.SetActive(false);
            GameObject.FindWithTag("MapButton").GetComponent<CanvasGroup>().alpha = 1.0f;
            mMappingButtonPanel.SetActive(true);
            mPlaneDetectionToggle.SetActive(true);
            Debug.Log("Started Session");
            if (!mappingStarted)
            {
                mappingStarted = true;
                LibPlacenote.Instance.StartSession();
            }
        }
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }*/
    }

    public void OnTogglePlaneDetection()
    {
        ConfigureSession(true);
        ARPlanePaintingStatus = mPlaneDetectionToggle.GetComponent<Toggle>().isOn;
        paintManager.paintOnTouch = !ARPlanePaintingStatus;
        paintManager.ARPlanePainting = ARPlanePaintingStatus;
    }

    private void StartARKit()
    {
        mLabelText.text = "Initializing ARKit";
        Application.targetFrameRate = 60;
        ConfigureSession(false);
    }

    private void ConfigureSession(bool clearPlanes)
    {
        ARKitWorldTrackingSessionConfiguration config =
            new ARKitWorldTrackingSessionConfiguration();

        if (mPlaneDetectionToggle.GetComponent<Toggle>().isOn)
        {
            if (UnityARSessionNativeInterface.IsARKit_1_5_Supported())
            {
                config.planeDetection = UnityARPlaneDetection.HorizontalAndVertical;
            }
            else
            {
                config.planeDetection = UnityARPlaneDetection.Horizontal;
            }
            mPNPlaneManager.StartPlaneDetection();
        }
        else
        {
            config.planeDetection = UnityARPlaneDetection.None;
            if (clearPlanes)
            {
                mPNPlaneManager.ClearPlanes();
            }
        }

        config.alignment = UnityARAlignment.UnityARAlignmentGravity;
        config.getPointCloudData = true;
        config.enableLightEstimation = true;
        mSession.RunWithConfig(config);
    }


    public void OnSaveMapClick()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            ToastManager.ShowToast("SDK not yet initialized", 2f);
            return;
        }

        OnDropPaintStrokeClick();

        bool useLocation = Input.location.status == LocationServiceStatus.Running;
        LocationInfo locationInfo = Input.location.lastData;
        // If there is a loaded map, then saving just updates the metadata
        if (!currentMapId.Equals(""))
        {
            mLabelText.text = "Setting MetaData...";
            SetMetaData(currentMapId);
        }
        else
        {
            mLabelText.text = "Saving...";
            LibPlacenote.Instance.SaveMap(
                (mapId) =>  // savedCb   upon saving the map locally
                {
                    LibPlacenote.Instance.StopSession();
                    mLabelText.text = "Saved Map ID: " + mapId;
                    mInitButtonPanel.SetActive(true);
                    //mMappingButtonPanel.SetActive(false);
                    //mPlaneDetectionToggle.SetActive(false);

                //clear all existing planes
                mPNPlaneManager.ClearPlanes();
                    mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;


                    JObject metadata = new JObject();

                    JObject shapeList = Shapes2JSON();
                    metadata["shapeList"] = shapeList;

                    JObject sv3list = Sv3s2JSON();
                    metadata["sv3list"] = sv3list;

                    JObject paintStrokeList = PaintStrokes2JSON();
                    metadata["paintStrokeList"] = paintStrokeList;


                    if (useLocation)
                    {
                        metadata["location"] = new JObject();
                        metadata["location"]["latitude"] = locationInfo.latitude;
                        metadata["location"]["longitude"] = locationInfo.longitude;
                        metadata["location"]["altitude"] = locationInfo.altitude;
                    }
                    else
                    { // default location so that JSON object is not invalid due to missing location data
                    metadata["location"] = new JObject();
                        metadata["location"]["latitude"] = 50.0f;
                        metadata["location"]["longitude"] = 100.0;
                        metadata["location"]["altitude"] = 10.0f;
                    }
                    LibPlacenote.Instance.SetMetadata(mapId, metadata);
                    currentMapId = mapId;
                },
                (completed, faulted, percentage) =>
                { // progressCb  upon transfer to cloud
                String percentText = (percentage * 100f).ToString();
                    uploadText.text = "Map upload status– Completed: " + completed + "    Faulted: " + faulted + "   " + "\n" + percentText + "% uploaded";
                    Debug.Log("faulted?: " + faulted);
                    Debug.Log("Completed?: " + completed);
                ActivateMapButton(false);
                }
            );
        }
    }

    public void OnClickUpdate() {
        if (currentMapId != "")
        {
            SetMetaData(currentMapId);
        }
    }

    private void SetMetaData(string mid) {
        OnDropPaintStrokeClick();

        bool useLocation = Input.location.status == LocationServiceStatus.Running;
        LocationInfo locationInfo = Input.location.lastData;

        JObject metadata = new JObject();

        JObject shapeList = Shapes2JSON();
        metadata["shapeList"] = shapeList;

        JObject sv3list = Sv3s2JSON();
        metadata["sv3list"] = sv3list;

        JObject paintStrokeList = PaintStrokes2JSON();
        metadata["paintStrokeList"] = paintStrokeList;


        if (useLocation)
        {
            metadata["location"] = new JObject();
            metadata["location"]["latitude"] = locationInfo.latitude;
            metadata["location"]["longitude"] = locationInfo.longitude;
            metadata["location"]["altitude"] = locationInfo.altitude;
        }
        else
        { // default location so that JSON object is not invalid due to missing location data
            metadata["location"] = new JObject();
            metadata["location"]["latitude"] = 50.0f;
            metadata["location"]["longitude"] = 100.0;
            metadata["location"]["altitude"] = 10.0f;
        }

        LibPlacenote.Instance.SetMetadata(mid, metadata);
    }

    private void SaveMeshes()
    {
        // TODO: Add the current PaintBrush as well
        GameObject[] meshes = GameObject.FindGameObjectsWithTag("Mesh");
        Mesh meshToSave = meshes[0].GetComponent<MeshFilter>().sharedMesh; // assumimg 1 exists for now
        ES3File es3File = new ES3File("testingMeshSave.es3");
        Debug.Log("meshToSave");
        Debug.Log(meshToSave);
        es3File.Save<Mesh>("Mesh", meshToSave);
        es3File.Sync();
        // Save your data to the ES3File.
        //es3File.Save<Transform>("myTransform", this.transform);
        //es3File.Save<string>("myName", myScript.name);

        // Get the ES3File as a string.
        string fileAsString = es3File.LoadRawString();
        Debug.Log(fileAsString.Length);
        Debug.Log(Application.persistentDataPath);
        ES3File es3fileLoading = new ES3File((new ES3Settings()).encoding.GetBytes(fileAsString), false);

        // Load the data from the ES3File.
        Mesh newMesh = new Mesh();
        es3fileLoading.LoadInto<Mesh>("Mesh", newMesh);
        GameObject newGO = new GameObject("newGO");
        newGO.AddComponent<MeshFilter>();
        newGO.GetComponent<MeshFilter>().sharedMesh = newMesh;
        newGO.transform.position = new Vector3(0.2f, 0.2f, 0.2f);

        //myScript.name = es3File.Load<string>("myName");


    }


    public void OnDropShapeClick()
    {
        Vector3 shapePosition = Camera.main.transform.position +
                                      Camera.main.transform.forward * 0.3f;
        Quaternion shapeRotation = Camera.main.transform.rotation;
        Debug.Log("Drop Shape @ Pos: " + shapePosition + ", Rot: " + shapeRotation);
        System.Random rnd = new System.Random();
        PrimitiveType type = (PrimitiveType)rnd.Next(0, 3);

        ShapeInfo shapeInfo = new ShapeInfo();
        shapeInfo.px = shapePosition.x;
        shapeInfo.py = shapePosition.y;
        shapeInfo.pz = shapePosition.z;
        shapeInfo.qx = shapeRotation.x;
        shapeInfo.qy = shapeRotation.y;
        shapeInfo.qz = shapeRotation.z;
        shapeInfo.qw = shapeRotation.w;
        shapeInfo.shapeType = type.GetHashCode();
        shapeInfoList.Add(shapeInfo);

        GameObject shape = ShapeFromInfo(shapeInfo);
        shapeObjList.Add(shape);
    }

    public void OnDropPaintStrokeClick()  // called when SaveMap is clicked. Add all the paint strokes to the lists at once
    {
        Debug.Log("1-OnDropPaintStrokeClick");
        paintStrokeObjList = paintManager.paintStrokesList;
        Debug.Log("2-OnDropPaintStrokeClick");
        // for each PaintStroke, convert to a PaintStrokeInfo, and add to paintStrokesInfoList
        if (paintStrokeObjList.Count > 0)
        {
            Debug.Log("3-OnDropPaintStrokeClick");
            foreach (var ps in paintStrokeObjList) // TODO: convert to for loop (?)
            {
                // Add the intialColor of the paintstroke
                Vector4 c = ps.color; // implicit conversion of Color to Vector4
                Debug.Log("4-OnDropPaintStrokeClick: " + c.x + " | " + c.y + " | " + c.z + " | " + c.w);
                PaintStrokeInfo psi = new PaintStrokeInfo();
                psi.initialColor = c; // implicit conversion of Vector4 to SerialiazableVector4  

                // Add the verts
                int vertCount = ps.verts.Count;
                SerializableVector3[] psiverts = new SerializableVector3[vertCount];
                psi.verts = psiverts;
                
                if (vertCount > 0)
                {
                    Debug.Log("5-OnDropPaintStrokeClick");
                    for (int j = 0; j < ps.verts.Count; j++)
                    {
                        //Debug.Log("6-OnDropPaintStrokeClick and ps.verts.Count is: " + ps.verts.Count);
                        //psi.verts[j] = new SerializableVector3(ps.verts[j].x, ps.verts[j].y, ps.verts[j].z);

                        psi.verts[j] = ps.verts[j]; // auto-conversion sv3 and Vector3
                    }
                    Debug.Log("7-OnDropPaintStrokeClick");
                    paintStrokeInfoList.Add(psi);
                    Debug.Log("8-OnDropPaintStrokeClick");
                }
            }
        }
    }

    private GameObject ShapeFromInfo(ShapeInfo info)
    {
        GameObject shape = GameObject.CreatePrimitive((PrimitiveType)info.shapeType);
        shape.transform.position = new Vector3(info.px, info.py, info.pz);
        shape.transform.rotation = new Quaternion(info.qx, info.qy, info.qz, info.qw);
        shape.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
        shape.GetComponent<MeshRenderer>().material = mShapeMaterial;

        return shape;
    }

    private void ClearShapes()
    {
        foreach (var obj in shapeObjList)
        {
            Destroy(obj);
        }
        shapeObjList.Clear();
        shapeInfoList.Clear();
    }

    private void ClearPaintStrokes()
    {
        foreach (var ps in paintStrokeObjList)
        {
            Destroy(ps);
        }
        paintStrokeObjList.Clear();
        paintStrokeInfoList.Clear();
    }

    private JObject PaintStrokes2JSON()
    {
        // Create a new PaintStrokeList with values copied from paintStrokesInfoList(a List of PaintStrokeInfo)
        // Despite the name, PaintStrokeList contains an array (not a List) of PaintStrokeInfo
        // Need this array to convert to a JObject

        /* // for reference:
        public class PaintStrokeList
        {
            public PaintStrokeInfo[] strokes;
        }
        public class PaintStrokeInfo
        {
            public SerializableVector3[] verts;
        }
        */

        PaintStrokeList psList = new PaintStrokeList();
        // define the array
        PaintStrokeInfo[] psiArray = new PaintStrokeInfo[paintStrokeInfoList.Count];
        psList.strokes = psiArray;
        // populate the array
        for (int i = 0; i < paintStrokeInfoList.Count; i++)
        {
            psiArray[i] = new PaintStrokeInfo();
            psiArray[i].verts = paintStrokeInfoList[i].verts;
            psiArray[i].initialColor = paintStrokeInfoList[i].initialColor;
        }

        return JObject.FromObject(psList);
    }

    private JObject Sv3s2JSON()
    {
        SV3List sV3List = new SV3List();
        //if (paintManager.currVertices.Count > 0)
        // for now, saving just the first PaintStroke
        int vertCount = paintManager.paintStrokesList[0].verts.Count;
        Debug.Log("vertCount" + vertCount);
        if (vertCount > 0)
        {
            sV3List.sv3s = new SerializableVector3[vertCount];
        }
        for (int i = 0; i < vertCount; i++)
        {
            sV3List.sv3s[i] = paintManager.paintStrokesList[0].verts[i];
        }

        //for (int i = 0; i < paintManager.paintStrokesList.Count; i++)
        //{
        //    int count = 
        //    for (int j = 0; i < paintManager.paintStrokesList[i].verts.Count; j++)
        //    {
        //        sV3List.sv3s[i] = paintManager.paintStrokesList[i].verts[j];

        //    }
        //    //sV3List.sv3s[i] = paintManager.currVertices[i];
        //}
        //sV3List.sv3s = new SerializableVector3[4];
        //sV3List.sv3s[0] = new SerializableVector3(1, 2, 3);
        //sV3List.sv3s[1] = new SerializableVector3(10, 2, 3);
        //sV3List.sv3s[2] = new SerializableVector3(1, 20, 3);
        //sV3List.sv3s[3] = new SerializableVector3(1, 2, 30);

        Debug.Log("XXXXXX: " + sV3List.sv3s.Length);
        JObject jo = JObject.FromObject(sV3List);
        Debug.Log("XXXXXX: " + jo);

        return JObject.FromObject(sV3List);
    }


    private JObject Shapes2JSON()
    {
        ShapeList shapeList = new ShapeList();
        shapeList.shapes = new ShapeInfo[shapeInfoList.Count];
        for (int i = 0; i < shapeInfoList.Count; i++)
        {
            shapeList.shapes[i] = shapeInfoList[i];
        }

        return JObject.FromObject(shapeList);
    }


    private void LoadSv3ListJSON(JToken mapMetadata)
    {
        if (paintManager)
        {
            //if (paintManager.currVertices)
            //{
            paintManager.currVertices.Clear();
            //}
        }
        if (mapMetadata is JObject && mapMetadata["sv3list"] is JObject)
        {
            SV3List sv3list = mapMetadata["sv3list"].ToObject<SV3List>();
            if (sv3list.sv3s == null)
            {
                Debug.Log("no sv3s dropped");
                return;
            }

            foreach (SerializableVector3 sv3 in sv3list.sv3s)
            {
                Vector3 vector = sv3;
                Debug.Log("YYYYY " + sv3);
                paintManager.currVertices.Add(vector);
            }
        }
    }


    private void LoadShapesJSON(JToken mapMetadata)
    {
        ClearShapes();

        if (mapMetadata is JObject && mapMetadata["shapeList"] is JObject)
        {
            ShapeList shapeList = mapMetadata["shapeList"].ToObject<ShapeList>();
            if (shapeList.shapes == null)
            {
                Debug.Log("no shapes dropped");
                return;
            }

            foreach (var shapeInfo in shapeList.shapes)
            {
                shapeInfoList.Add(shapeInfo);
                GameObject shape = ShapeFromInfo(shapeInfo);
                shapeObjList.Add(shape);
            }
        }
    }

    private void LoadPaintStrokesJSON(JToken mapMetadata)
    {
        ClearPaintStrokes(); // Clear the paintstrokes

        if (mapMetadata is JObject && mapMetadata["paintStrokeList"] is JObject)
        {
            PaintStrokeList paintStrokes = mapMetadata["paintStrokeList"].ToObject<PaintStrokeList>();
            if (paintStrokes.strokes == null)
            {
                Debug.Log("no PaintStrokes were added");
                return;
            }

            // (may need to do a for loop to ensure they stay in order?)
            foreach (var paintInfo in paintStrokes.strokes)
            {
                paintStrokeInfoList.Add(paintInfo);
                PaintStroke paintstroke = PaintStrokeFromInfo(paintInfo);
                paintStrokeObjList.Add(paintstroke); // should be used by PaintManager to recreate painting
            }
            paintManager.paintStrokesList = paintStrokeObjList; // not really objects, rather components
            paintManager.RecreatePaintedStrokes();
        }
    }

    private void TestPaintStrokeInfo() {
        PaintStrokeInfo psi = new PaintStrokeInfo();
        psi.verts = new SerializableVector3[3];
        psi.verts[0] = new SerializableVector3(1, 2, 3);
        psi.verts[1] = new SerializableVector3(10, 2, 3);
        psi.verts[2] = new SerializableVector3(1, 20, 3);
        PaintStroke ps = PaintStrokeFromInfo(psi);
        Debug.Log("ps: ?????????:");
        Debug.Log(ps);
    }

    private PaintStroke PaintStrokeFromInfo(PaintStrokeInfo info)
    {
        //TODO: Won't work with 'new' because PaintStroke is a monobehavior. Probably better if PaintStroke is not a monobehavior
        //PaintStroke paintStroke = new PaintStroke();
        GameObject psHolder = new GameObject("psholder");
        psHolder.AddComponent<PaintStroke>();
        PaintStroke paintStroke = psHolder.GetComponent<PaintStroke>();
        paintStroke.color = new Color(info.initialColor.x, info.initialColor.y, info.initialColor.z, info.initialColor.w);
        List<Vector3> v = new List<Vector3>();
        paintStroke.verts = v;
        for (int i = 0; i < info.verts.Length; i++)
        {
            //paintStroke.verts[i] = info.verts[i];  // implicit conversion of SV3 to Vector3
            paintStroke.verts.Add(info.verts[i]);
        }

        return paintStroke;
    }


    public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }


    public void OnStatusChange(LibPlacenote.MappingStatus prevStatus,
                                LibPlacenote.MappingStatus currStatus)
    {
        //Debug.Log("VERSION?: ");
        //Debug.Log(mSelectedMapInfo.userData["version"]["a"].ToObject<float>());
        Debug.Log("prevStatus: " + prevStatus.ToString() +
                   " currStatus: " + currStatus.ToString());
        if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
            prevStatus == LibPlacenote.MappingStatus.LOST)
        {
            if (!hasLocalized)
            {
                mLabelText.text = "Localized";
                LoadShapesJSON(mSelectedMapInfo.userData);
                LoadSv3ListJSON(mSelectedMapInfo.userData);
                LoadPaintStrokesJSON(mSelectedMapInfo.userData);
                Debug.Log("metadata:");
                Debug.Log(mSelectedMapInfo.userData);
                hasLocalized = true;
            }
        }
        else if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
                 prevStatus == LibPlacenote.MappingStatus.WAITING)
        {
            mLabelText.text = "Mapping";
        }
        else if (currStatus == LibPlacenote.MappingStatus.LOST)
        {
            mLabelText.text = "Searching for position lock";
        }
        else if (currStatus == LibPlacenote.MappingStatus.WAITING)
        {
            if (shapeObjList.Count != 0)
            {
                ClearShapes();
            }
            OnNewMapClick(); // start session automatically
        }
    }

}
