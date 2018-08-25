//// Updated for Placenote 1.6.2

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using System;
//using UnityEngine.UI;
//using UnityEngine.XR.iOS;
//using System.Runtime.InteropServices;
//using Newtonsoft.Json.Linq;
//using Newtonsoft.Json;

//[Serializable]
//public class ShapeInfo
//{
//    public float px;
//    public float py;
//    public float pz;
//    public float qx;
//    public float qy;
//    public float qz;
//    public float qw;
//    public int shapeType;
//}

//[Serializable]
//public class ShapeList
//{
//    public ShapeInfo[] shapes;
//}

////[Serializable]
////public class PaintStrokeInfo
////{
////    public SerializableVector3[] verts;
////    public SerializableVector3[] pointColors; // alpha is always 1, and using V3 avoids deserialization problems with V4
////    public float[] pointSizes;
////    public SerializableVector4 initialColor; // initial color of stroke. Color implicitly converts to Vector4.
////}

////[Serializable]
////public class PaintStrokeList
////{
////    public PaintStrokeInfo[] strokes;
////}

//public class LeavesManager : MonoBehaviour, PlacenoteListener
//{
//    public GameObject modelPrefab;
//    public Vector3 paintPosition;
//    [SerializeField] Material mShapeMaterial;

//    // Get refs to buttons in the UI
//    private GameObject mMapLoader;
//    private GameObject mExitButton;
//    private GameObject mListElement;
//    private RectTransform mListContentParent;
//    private ToggleGroup mToggleGroup; // Toggle Group for the Toggles in each list element menu item
//    private GameObject mPlaneDetectionToggle;
//    private Text mLabelText;
//    private Text uploadText;
//    private GameObject mapButton;
//    private PlacenoteARGeneratePlane mPNPlaneManager;

//    private UnityARSessionNativeInterface mSession;
//    private bool mFrameUpdated = false;
//    private UnityARImageFrameData mImage = null;
//    private UnityARCamera mARCamera;
//    private bool mARKitInit = false;
//    private List<ShapeInfo> shapeInfoList = new List<ShapeInfo>();
//    private List<GameObject> shapeObjList = new List<GameObject>();
//    private List<PaintStrokeInfo> paintStrokeInfoList = new List<PaintStrokeInfo>();
//    private List<PaintStroke> paintStrokeObjList = new List<PaintStroke>();

//    private PaintManager paintManager;
//    private SerializeModels sModels;
//    private SerializePaintStrokes sPaintStrokes;

//    //New stuff with PN 1.62
//    private Slider mRadiusSlider;
//    private float defaultDistance = 8000f;
//    private LibPlacenote.MapMetadataSettable mCurrMapDetails;
//    private bool mReportDebug = false;
//    private string mSaveMapId = null;

//    private LibPlacenote.MapInfo mSelectedMapInfo;

//    private bool hasLocalized; // flag to prevent continually reloading the metadata when position is lost and regained
//    private bool mappingStarted;
//    private string currentMapId;

//    private Coroutine pulseMapButton;

//    private string mSelectedMapId
//    {
//        get
//        {
//            return
//            mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
//        }
//    }

//    bool ARPlanePaintingStatus;

//    void Start()
//    {
//        InitUI();

//        // Set up SerializableModel's
//        sModels = ScriptableObject.CreateInstance<SerializeModels>();
//        sModels.Init();
//        sModels.prefabs[0] = modelPrefab;

//        sPaintStrokes = ScriptableObject.CreateInstance<SerializePaintStrokes>();
//        sPaintStrokes.Init();

//        currentMapId = "";
//        mappingStarted = false;
//        hasLocalized = false;
//        mPNPlaneManager = GameObject.FindWithTag("PNPlaneManager").GetComponent<PlacenoteARGeneratePlane>();

//        Input.location.Start();

//        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
//        UnityARSessionNativeInterface.ARFrameUpdatedEvent += ARFrameUpdated;
//        StartARKit();
//        FeaturesVisualizer.EnablePointcloud();
//        LibPlacenote.Instance.RegisterListener(this);

//        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
//        ARPlanePaintingStatus = mPlaneDetectionToggle.GetComponent<Toggle>().isOn;
//        paintManager.ARPlanePainting = ARPlanePaintingStatus;
//        paintManager.paintOnTouch = !ARPlanePaintingStatus; // TODO: make an enum to replace multiple bools

//    }

//    private void InitUI()
//    {
//        mMapLoader = GameObject.FindWithTag("MapLoader");
//        mExitButton = GameObject.FindWithTag("ExitMapButton");
//        mListElement = GameObject.FindWithTag("MapInfoElement");

//        GameObject ListParent = GameObject.FindWithTag("ListContentParent");
//        mListContentParent = ListParent.GetComponent<RectTransform>();
//        mToggleGroup = ListParent.GetComponent<ToggleGroup>();

//        mPlaneDetectionToggle = GameObject.FindWithTag("PlaneDetectionToggle");
//        mLabelText = GameObject.FindWithTag("LabelText").GetComponent<Text>();
//        uploadText = GameObject.FindWithTag("UploadText").GetComponent<Text>();
//        mapButton = GameObject.FindWithTag("MapButton");
//        mRadiusSlider = GameObject.FindWithTag("RadiusSlider").GetComponent<Slider>();
//        ResetSlider();
//        mMapLoader.SetActive(false); // needs to be active at Start, so the reference to it can be found
//    }

//    // force Ahead Of Time compiling of List<SerializableVector3> with this unused method
//    // Fixes error in deserializing List of SerializableVector3
//    // (alternative fix -- could define SerializableVector3 as class instead of struct)
//    private void dummyMethod()
//    {
//        List<SerializableVector3> forceAOT = new List<SerializableVector3>();
//    }

//    private void ARFrameUpdated(UnityARCamera camera)
//    {
//        mFrameUpdated = true;
//        mARCamera = camera;
//    }

//    private void InitARFrameBuffer()
//    {
//        mImage = new UnityARImageFrameData();

//        int yBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yHeight;
//        mImage.y.data = Marshal.AllocHGlobal(yBufSize);
//        mImage.y.width = (ulong)mARCamera.videoParams.yWidth;
//        mImage.y.height = (ulong)mARCamera.videoParams.yHeight;
//        mImage.y.stride = (ulong)mARCamera.videoParams.yWidth;

//        // This does assume the YUV_NV21 format
//        int vuBufSize = mARCamera.videoParams.yWidth * mARCamera.videoParams.yWidth / 2;
//        mImage.vu.data = Marshal.AllocHGlobal(vuBufSize);
//        mImage.vu.width = (ulong)mARCamera.videoParams.yWidth / 2;
//        mImage.vu.height = (ulong)mARCamera.videoParams.yHeight / 2;
//        mImage.vu.stride = (ulong)mARCamera.videoParams.yWidth;

//        mSession.SetCapturePixelData(true, mImage.y.data, mImage.vu.data);
//    }

//    void Update()
//    {
//        if (!mappingStarted) // start mapping automatically
//        {
//            OnNewMapClick();
//        }
//        if (mFrameUpdated)
//        {
//            mFrameUpdated = false;
//            if (mImage == null)
//            {
//                InitARFrameBuffer();
//            }

//            if (mARCamera.trackingState == ARTrackingState.ARTrackingStateNotAvailable)
//            {
//                // ARKit pose is not yet initialized
//                return;
//            }
//            else if (!mARKitInit)
//            {
//                mARKitInit = true;
//                mLabelText.text = "ARKit Initialized";
//                if (!LibPlacenote.Instance.Initialized())
//                {
//                    Debug.Log("initialized Mapping");


//                }

//            }

//            Matrix4x4 matrix = mSession.GetCameraPose();

//            Vector3 arkitPosition = PNUtility.MatrixOps.GetPosition(matrix);
//            Quaternion arkitQuat = PNUtility.MatrixOps.GetRotation(matrix);

//            LibPlacenote.Instance.SendARFrame(mImage, arkitPosition, arkitQuat,
//                                              mARCamera.videoParams.screenOrientation);

//        }

//        paintPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
//        if (mappingStarted)
//        {
//            pulseMapButton = StartCoroutine(PulseColor(mapButton.GetComponent<Image>()));
//        }
//    }

//    private void ActivateMapButton(bool mappingOn)
//    {
//        Image imageComponent = mapButton.GetComponent<Image>();
//        imageComponent.fillCenter = mappingOn;
//        if (mappingOn)
//        {
//            mapButton.GetComponent<CanvasGroup>().alpha = 1.0f;
//            imageComponent.color = Color.yellow;
//            //pulseMapButton = StartCoroutine(PulseColor(imageComponent));
//        }
//        else
//        {
//            mapButton.GetComponent<CanvasGroup>().alpha = 0.4f;
//            StopCoroutine(pulseMapButton);
//            imageComponent.color = Color.white;
//        }
//    }

//    // Make a color pulsate
//    private IEnumerator PulseColor(Image img)
//    {
//        float alpha = (Mathf.Sin(Time.time * 2f) + 1.9f) * 0.33f;
//        img.color = new Color(1.0f, 0.95f, 0.6f, alpha);

//        yield return null;
//    }

//    //TODO: Use search map instead (ListMap downloads the entire map + metadata for all maps, very inefficient)
//    public void OnListMapClick()
//    {
//        if (!LibPlacenote.Instance.Initialized())
//        {
//            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
//            return;
//        }

//        //foreach (Transform t in mListContentParent.transform)
//        //{
//        //    // TODO: check if this destroy command is a cause of the slow loading of the list
//        //    Destroy(t.gameObject);
//        //}
//        mMapLoader.SetActive(true);
//        //mMapListPanel.SetActive(true);

//        //        mInitButtonPanel.SetActive(false); // added in 1.62

//        //        mRadiusSlider.gameObject.SetActive(true);

//        LibPlacenote.Instance.ListMaps((mapList) =>
//        {
//            //Debug.Log("MAPID INFO'S HOW MANY: " + mapList[0].metadata.ToString()); 
//            // render the map list!
//            foreach (LibPlacenote.MapInfo mapId in mapList)
//            {

//                if (mapId.metadata != null) // extra if, can be removed, prevent editor warning
//                {
//                    if (mapId.metadata.userdata != null)
//                    {
//                        //Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
//                    }
//                    AddMapToList(mapId);
//                }
//            }
//        });
//    }

//    // Radius stuff - new with PN 1.62
//    public void OnRadiusSelect()
//    {
//        LocationInfo locationInfo = Input.location.lastData;
//        Debug.Log(locationInfo);

//        float radiusSearch = mRadiusSlider.value;// * mMaxRadiusSearch;
//        //mRadiusLabel.text = "Distance Filter: " + (radiusSearch / 1000.0).ToString("F2") + " km";

//        LibPlacenote.Instance.SearchMaps(locationInfo.latitude, locationInfo.longitude, radiusSearch,
//            (mapList) =>
//            {
//                //                Debug.Log("MAPID INFO'S HOW much: " + mapList[0].metadata.ToString()); 

//                foreach (Transform t in mListContentParent.transform)
//                {
//                    Destroy(t.gameObject);
//                }
//                // render the map list!
//                foreach (LibPlacenote.MapInfo mapId in mapList)
//                {
//                    Debug.Log(mapId);
//                    if (mapId.metadata != null) // extra if statement just prevents warning in Editor
//                    {
//                        if (mapId.metadata.userdata != null)
//                        {
//                            Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
//                        }
//                        AddMapToList(mapId);
//                    }
//                }
//            });
//    }

//    public void ResetSlider()
//    {
//        mRadiusSlider.value = defaultDistance;
//        //mRadiusLabel.text = "Distance Filter: Off";
//    }


//    public void OnCancelClick()
//    {
//        mMapLoader.SetActive(false);
//        ResetSlider();
//    }


//    public void OnExitClick()
//    {
//        paintManager.Reset();
//        //        mInitButtonPanel.SetActive(true);
//        mExitButton.SetActive(false);
//        mPlaneDetectionToggle.SetActive(false);

//        //clear all existing planes
//        mPNPlaneManager.ClearPlanes();
//        mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;

//        LibPlacenote.Instance.StopSession();
//        mappingStarted = false; // allow a new mapping session to begin auto. in Update
//        ActivateMapButton(false); // Should change to true in NewMapClicked
//        hasLocalized = false;

//    }


//    void AddMapToList(LibPlacenote.MapInfo mapInfo)
//    {
//        GameObject newElement = Instantiate(mListElement) as GameObject;
//        MapInfoElement listElement = newElement.GetComponent<MapInfoElement>();
//        listElement.Initialize(mapInfo, mToggleGroup, mListContentParent, (value) =>
//        {
//            OnMapSelected(mapInfo);
//        });
//    }


//    void OnMapSelected(LibPlacenote.MapInfo mapInfo)
//    {
//        mSelectedMapInfo = mapInfo;
//        //mMapSelectedPanel.SetActive(true);
//        mMapLoader.SetActive(true);
//        mRadiusSlider.gameObject.SetActive(false);
//    }


//    public void OnLoadMapClicked()
//    {
//        ConfigureSession(false);
//        // Since a session starts running after app launch automatically,
//        // ensure that if a session is already running, it is stopped
//        LibPlacenote.Instance.StopSession();
//        ActivateMapButton(false);
//        paintManager.Reset();
//        hasLocalized = false; // reset flag that limits localization to just once
//        if (!LibPlacenote.Instance.Initialized())
//        {
//            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
//            return;
//        }

//        hasLocalized = false;
//        mLabelText.text = "Loading Map ID: " + mSelectedMapId;
//        LibPlacenote.Instance.LoadMap(mSelectedMapId,
//            (completed, faulted, percentage) =>
//            {
//                if (completed)
//                {
//                    mMapLoader.SetActive(false);

//                    LibPlacenote.Instance.StartSession();
//                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
//                    currentMapId = mSelectedMapId;

//                    if (mReportDebug)
//                    {
//                        LibPlacenote.Instance.StartRecordDataset(
//                            (datasetCompleted, datasetFaulted, datasetPercentage) =>
//                            {

//                                if (datasetCompleted)
//                                {
//                                    mLabelText.text = "Dataset Upload Complete";
//                                }
//                                else if (datasetFaulted)
//                                {
//                                    mLabelText.text = "Dataset Upload Faulted";
//                                }
//                                else
//                                {
//                                    mLabelText.text = "Dataset Upload: " + datasetPercentage.ToString("F2") + "/1.0";
//                                }
//                            });
//                        Debug.Log("Started Debug Report");
//                    }
//                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
//                }
//                else if (faulted)
//                {
//                    mLabelText.text = "Failed to load ID: " + mSelectedMapId;
//                }
//                else
//                {
//                    mLabelText.text = "Map Download: " + percentage.ToString("F2") + "/1.0";
//                }
//            }
//        );
//    }

//    public void OnDeleteMapClicked()
//    {
//        if (!LibPlacenote.Instance.Initialized())
//        {
//            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
//            return;
//        }

//        mLabelText.text = "Deleting Map ID: " + mSelectedMapId;
//        LibPlacenote.Instance.DeleteMap(mSelectedMapId, (deleted, errMsg) =>
//        {
//            if (deleted)
//            {
//                //mMapSelectedPanel.SetActive(false);
//                mLabelText.text = "Deleted ID: " + mSelectedMapId;
//                OnListMapClick();
//            }
//            else
//            {
//                mLabelText.text = "Failed to delete ID: " + mSelectedMapId;
//            }
//        });
//    }


//    public void OnNewMapClick()
//    {
//        ConfigureSession(false);

//        if (LibPlacenote.Instance.Initialized())
//        {
//            if (!mappingStarted)
//            {
//                GameObject.FindWithTag("MapButton").GetComponent<CanvasGroup>().alpha = 1.0f;
//                //mMappingButtonPanel.SetActive(true);
//                mPlaneDetectionToggle.SetActive(true);
//                Debug.Log("Started Session");
//                mappingStarted = true;
//                ActivateMapButton(mappingStarted);
//                LibPlacenote.Instance.StartSession();
//            }

//            if (mReportDebug)
//            {
//                LibPlacenote.Instance.StartRecordDataset(
//                    (completed, faulted, percentage) =>
//                    {
//                        if (completed)
//                        {
//                            mLabelText.text = "Dataset Upload Complete";
//                        }
//                        else if (faulted)
//                        {
//                            mLabelText.text = "Dataset Upload Faulted";
//                        }
//                        else
//                        {
//                            mLabelText.text = "Dataset Upload: (" + percentage.ToString("F2") + "/1.0)";
//                        }
//                    });
//                Debug.Log("Started Debug Report");
//            }
//        }
//    }

//    public void OnTogglePlaneDetection()
//    {
//        ConfigureSession(true);
//        ARPlanePaintingStatus = mPlaneDetectionToggle.GetComponent<Toggle>().isOn;
//        paintManager.paintOnTouch = !ARPlanePaintingStatus;
//        paintManager.ARPlanePainting = ARPlanePaintingStatus;
//    }

//    private void StartARKit()
//    {
//        mLabelText.text = "Initializing ARKit";
//        Application.targetFrameRate = 60;
//        ConfigureSession(false);
//    }

//    private void ConfigureSession(bool clearPlanes)
//    {
//#if !UNITY_EDITOR
//        ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();

//        if (mPlaneDetectionToggle.GetComponent<Toggle>().isOn)
//        {
//            if (UnityARSessionNativeInterface.IsARKit_1_5_Supported())
//            {
//                config.planeDetection = UnityARPlaneDetection.HorizontalAndVertical;
//            }
//            else
//            {
//                config.planeDetection = UnityARPlaneDetection.Horizontal;
//            }
//            mPNPlaneManager.StartPlaneDetection();
//        }
//        else
//        {
//            config.planeDetection = UnityARPlaneDetection.None;
//            if (clearPlanes)
//            {
//                mPNPlaneManager.ClearPlanes();
//            }
//        }

//        config.alignment = UnityARAlignment.UnityARAlignmentGravity;
//        config.getPointCloudData = true;
//        config.enableLightEstimation = true;
//        mSession.RunWithConfig(config);
//#endif
//    }


//    public void OnSaveMapClick()
//    {
//        if (!LibPlacenote.Instance.Initialized())
//        {
//            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
//            return;
//        }

//        OnDropPaintStrokeClick();

//        bool useLocation = Input.location.status == LocationServiceStatus.Running;
//        LocationInfo locationInfo = Input.location.lastData;
//        // If there is a loaded map, then saving just updates the metadata
//        if (!currentMapId.Equals(""))
//        {
//            mLabelText.text = "Setting MetaData...";
//            SetMetaData(currentMapId);
//        }
//        else
//        {
//            mLabelText.text = "Saving...";
//            LibPlacenote.Instance.SaveMap(
//                (mapId) =>  // savedCb   upon saving the map locally
//                {
//                    LibPlacenote.Instance.StopSession();
//                    mLabelText.text = "Saved Map ID: " + mapId;

//                    //clear all existing planes
//                    mPNPlaneManager.ClearPlanes();
//                    mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;

//                    // Updated for 1.62
//                    LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();
//                    metadata.name = RandomName.Get();
//                    mLabelText.text = "Saved Map Name: " + metadata.name;
//                    JObject userdata = new JObject();
//                    metadata.userdata = userdata;
//                    //

//                    //JObject metadata = new JObject();
                    
//                    userdata[sModels.jsonKey] = sModels.ToJSON(); // replaces shapeList

//                    //JObject shapeList = Shapes2JSON();
//                    //userdata["shapeList"] = shapeList;

//                    //JObject paintStrokeList = PaintStrokes2JSON();
//                    //userdata["paintStrokeList"] = paintStrokeList;
//                    userdata[sPaintStrokes.jsonKey] = sPaintStrokes.ToJSON();

//                    if (useLocation)
//                    {
//                        metadata.location = new LibPlacenote.MapLocation();
//                        metadata.location.latitude = locationInfo.latitude;
//                        metadata.location.longitude = locationInfo.longitude;
//                        metadata.location.altitude = locationInfo.altitude;
//                    }
//                    else
//                    { // default location so that JSON object is not invalid due to missing location data
//                        metadata.location = new LibPlacenote.MapLocation();
//                        metadata.location.latitude = 50f;
//                        metadata.location.longitude = 100f;
//                        metadata.location.altitude = 10f;
//                    }
//                    LibPlacenote.Instance.SetMetadata(mapId, metadata);
//                    currentMapId = mCurrMapDetails.name; //mapId;  // from prev. PN version, still needed?
//                    mCurrMapDetails = metadata;
//                },
//                (completed, faulted, percentage) =>
//                { // progressCb  upon transfer to cloud
//                    String percentText = (percentage * 100f).ToString();
//                    uploadText.text = "Map upload status– Completed: " + completed + "    Faulted: " + faulted + "   " + "\n" + percentText + "% uploaded";

//                    ActivateMapButton(false);
//                }
//            );
//        }
//    }

//    public void OnClickUpdate()
//    {
//        if (currentMapId != "")
//        {
//            Debug.Log(currentMapId);
//            SetMetaData(currentMapId);
//        }
//    }

//    //TODO: update for 1.62
//    private void SetMetaData(string mid)
//    {
//       // OnDropPaintStrokeClick();
//        sPaintStrokes.OnAddToScene();

//        bool useLocation = Input.location.status == LocationServiceStatus.Running;
//        LocationInfo locationInfo = Input.location.lastData;

//        // Update for PN 1.62
//        LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();
//        metadata.name = RandomName.Get();
//        mLabelText.text = "Saved Map Name: " + metadata.name;
//        JObject userdata = new JObject();
//        metadata.userdata = userdata;

//        //JObject shapeList = Shapes2JSON();
//        //userdata["shapeList"] = shapeList;

//        userdata[sModels.jsonKey] = sModels.ToJSON();

//        //JObject paintStrokeList = PaintStrokes2JSON();
//        //userdata["paintStrokeList"] = paintStrokeList;
//        userdata[sPaintStrokes.jsonKey] = sPaintStrokes.ToJSON();


//        if (useLocation)
//        {
//            metadata.location = new LibPlacenote.MapLocation();
//            metadata.location.latitude = locationInfo.latitude;
//            metadata.location.longitude = locationInfo.longitude;
//            metadata.location.altitude = locationInfo.altitude;
//        }
//        else
//        { // default location so that JSON object is not invalid due to missing location data
//            metadata.location = new LibPlacenote.MapLocation();
//            metadata.location.latitude = 50f;
//            metadata.location.longitude = 100f;
//            metadata.location.altitude = 10f;
//        }

//        LibPlacenote.Instance.SetMetadata(mid, metadata);
//    }

//    public void OnDropShapeClick()
//    {
//        sModels.OnAddToScene();
//        //Vector3 shapePosition = Camera.main.transform.position + Camera.main.transform.forward * 1.3f;// + new Vector3(0f,0f,0.5f);
//        //Quaternion shapeRotation = Camera.main.transform.rotation;
//        //Debug.Log("Drop Shape @ Pos: " + shapePosition + ", Rot: " + shapeRotation);
//        //System.Random rnd = new System.Random();
//        //PrimitiveType type = (PrimitiveType)rnd.Next(0, 3);

//        //ShapeInfo shapeInfo = new ShapeInfo();
//        //shapeInfo.px = shapePosition.x;
//        //shapeInfo.py = shapePosition.y;
//        //shapeInfo.pz = shapePosition.z;
//        //shapeInfo.qx = shapeRotation.x;
//        //shapeInfo.qy = shapeRotation.y;
//        //shapeInfo.qz = shapeRotation.z;
//        //shapeInfo.qw = shapeRotation.w;
//        //shapeInfo.shapeType = type.GetHashCode();
//        //shapeInfoList.Add(shapeInfo);

//        //GameObject shape = ModelFromInfo(shapeInfo);
//        //shapeObjList.Add(shape);
//    }

//    public void OnDropPaintStrokeClick()  // called when SaveMap is clicked. Add all the paint strokes to the lists at once
//    {
//        sPaintStrokes.OnAddToScene();
//        //Debug.Log("1-OnDropPaintStrokeClick");
//        //paintStrokeObjList = paintManager.paintStrokesList;
//        //Debug.Log("2-OnDropPaintStrokeClick");
//        //// for each PaintStroke, convert to a PaintStrokeInfo, and add to paintStrokesInfoList
//        //if (paintStrokeObjList.Count > 0)
//        //{
//        //    Debug.Log("3-OnDropPaintStrokeClick");
//        //    foreach (var ps in paintStrokeObjList) // TODO: convert to for loop (?)
//        //    {
//        //        // Add the intialColor of the paintstroke
//        //        Vector4 c = ps.color; // implicit conversion of Color to Vector4
//        //        Debug.Log("4-OnDropPaintStrokeClick: " + c.x + " | " + c.y + " | " + c.z + " | " + c.w);
//        //        PaintStrokeInfo psi = new PaintStrokeInfo();
//        //        psi.initialColor = c; // implicit conversion of Vector4 to SerialiazableVector4  

//        //        // Add the verts
//        //        int vertCount = ps.verts.Count;
//        //        //todo: combine in 1 line?
//        //        SerializableVector3[] psiverts = new SerializableVector3[vertCount];
//        //        psi.verts = psiverts;

//        //        // Add the colors per point
//        //        SerializableVector3[] psicolors = new SerializableVector3[vertCount];
//        //        psi.pointColors = psicolors;
//        //        Debug.Log("psi.pointColors length: " + psi.pointColors.Length);

//        //        // Add the size per point
//        //        psi.pointSizes = new float[vertCount];


//        //        if (vertCount > 0)
//        //        {
//        //            Debug.Log("5-OnDropPaintStrokeClick");
//        //            for (int j = 0; j < vertCount; j++)
//        //            {
//        //                Debug.Log("6-OnDropPaintStrokeClick and ps.verts.Count is: " + ps.verts.Count);
//        //                //psi.verts[j] = new SerializableVector3(ps.verts[j].x, ps.verts[j].y, ps.verts[j].z);

//        //                psi.verts[j] = ps.verts[j]; // auto-conversion sv3 and Vector3
//        //                Debug.Log("6.5-OnDropPaintStrokeClick");
//        //                //Vector4 vector4color = ps.pointColors[j]; // implicit conversion of Color to Vector4
//        //                psi.pointColors[j] = new Vector3(ps.pointColors[j].r, ps.pointColors[j].g, ps.pointColors[j].b);
//        //                psi.pointSizes[j] = ps.pointSizes[j];
//        //            }
//        //            Debug.Log("7-OnDropPaintStrokeClick");
//        //            paintStrokeInfoList.Add(psi);
//        //            Debug.Log("8-OnDropPaintStrokeClick");
//        //        }
//        //    }
//        //}
//    }

//    private GameObject ShapeFromInfo(ShapeInfo info)
//    {
//        GameObject shape = GameObject.CreatePrimitive((PrimitiveType)info.shapeType);
//        shape.transform.position = new Vector3(info.px, info.py, info.pz);
//        shape.transform.rotation = new Quaternion(info.qx, info.qy, info.qz, info.qw);
//        shape.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
//        shape.GetComponent<MeshRenderer>().material = mShapeMaterial;

//        return shape;
//    }

//    // Add a custom 3D model to the map
//    private GameObject ModelFromInfo(ShapeInfo info)
//    {
//        Vector3 pos = new Vector3(info.px, info.py, info.pz);
//        Quaternion rot = new Quaternion(info.qx, info.qy, info.qz, info.qw);
//        Vector3 localScale = new Vector3(0.05f, 0.05f, 0.05f);
//        GameObject model = Instantiate(modelPrefab, pos, rot);

//        return model;
//    }

//    private void ClearShapes()
//    {
//        foreach (var obj in shapeObjList)
//        {
//            Destroy(obj);
//        }
//        shapeObjList.Clear();
//        shapeInfoList.Clear();
//    }

//    private void ClearPaintStrokes()
//    {
//        foreach (var ps in paintStrokeObjList)
//        {
//            Destroy(ps);
//        }
//        paintStrokeObjList.Clear();
//        paintStrokeInfoList.Clear();
//    }

//    private JObject PaintStrokes2JSON()
//    {
//        // Create a new PaintStrokeList with values copied from paintStrokesInfoList(a List of PaintStrokeInfo)
//        // Despite the name, PaintStrokeList contains an array (not a List) of PaintStrokeInfo
//        // Need this array to convert to a JObject

//        PaintStrokeList psList = new PaintStrokeList();
//        // define the array
//        PaintStrokeInfo[] psiArray = new PaintStrokeInfo[paintStrokeInfoList.Count];
//        psList.strokes = psiArray;
//        // populate the array
//        for (int i = 0; i < paintStrokeInfoList.Count; i++)
//        {
//            psiArray[i] = new PaintStrokeInfo();
//            psiArray[i].verts = paintStrokeInfoList[i].verts;
//            psiArray[i].pointColors = paintStrokeInfoList[i].pointColors;
//            psiArray[i].pointSizes = paintStrokeInfoList[i].pointSizes;
//            psiArray[i].initialColor = paintStrokeInfoList[i].initialColor;
//        }

//        return JObject.FromObject(psList);
//    }


//    private JObject Shapes2JSON()
//    {
//        ShapeList shapeList = new ShapeList();
//        shapeList.shapes = new ShapeInfo[shapeInfoList.Count];
//        for (int i = 0; i < shapeInfoList.Count; i++)
//        {
//            shapeList.shapes[i] = shapeInfoList[i];
//        }

//        return JObject.FromObject(shapeList);
//    }

//    private void LoadShapesJSON(JToken mapMetadata)
//    {
//        ClearShapes();

//        if (mapMetadata is JObject && mapMetadata["shapeList"] is JObject)
//        {
//            ShapeList shapeList = mapMetadata["shapeList"].ToObject<ShapeList>();
//            if (shapeList.shapes == null)
//            {
//                Debug.Log("no shapes dropped");
//                return;
//            }

//            foreach (var shapeInfo in shapeList.shapes)
//            {
//                shapeInfoList.Add(shapeInfo);
//                //GameObject shape = ShapeFromInfo(shapeInfo);
//                GameObject shape = ModelFromInfo(shapeInfo);
//                shapeObjList.Add(shape);
//            }
//        }
//    }

//    private void LoadPaintStrokesJSON(JToken mapMetadata)
//    {
//        ClearPaintStrokes(); // Clear the paintstrokes

//        if (mapMetadata is JObject && mapMetadata["paintStrokeList"] is JObject)
//        {
//            Debug.Log("A-LoadPaintStrokesJSON");
//            // this next line breaks when deserializing a list of vector4's
//            PaintStrokeList paintStrokes = mapMetadata["paintStrokeList"].ToObject<PaintStrokeList>();
//            Debug.Log("B-LoadPaintStrokesJSON");
//            if (paintStrokes.strokes == null)
//            {
//                Debug.Log("no PaintStrokes were added");
//                return;
//            }

//            // (may need to do a for loop to ensure they stay in order?)
//            foreach (var paintInfo in paintStrokes.strokes)
//            {
//                paintStrokeInfoList.Add(paintInfo);
//                PaintStroke paintstroke = PaintStrokeFromInfo(paintInfo);
//                paintStrokeObjList.Add(paintstroke); // should be used by PaintManager to recreate painting
//                Debug.Log("C-LoadPaintStrokesJSON");
//            }
//            Debug.Log("D-LoadPaintStrokesJSON");
//            paintManager.paintStrokesList = paintStrokeObjList; // not really objects, rather components
//            Debug.Log("E-LoadPaintStrokesJSON");
//            paintManager.RecreatePaintedStrokes();
//            Debug.Log("F-LoadPaintStrokesJSON");
//        }
//    }

//    private PaintStroke PaintStrokeFromInfo(PaintStrokeInfo info)
//    {
//        //TODO: Won't work with 'new' because PaintStroke is a monobehavior. Probably better if PaintStroke is not a monobehavior
//        //PaintStroke paintStroke = new PaintStroke();
//        GameObject psHolder = new GameObject("psholder");
//        psHolder.AddComponent<PaintStroke>();
//        PaintStroke paintStroke = psHolder.GetComponent<PaintStroke>();
//        paintStroke.color = new Color(info.initialColor.x, info.initialColor.y, info.initialColor.z, info.initialColor.w);
//        List<Vector3> v = new List<Vector3>();
//        paintStroke.verts = v;
//        List<Color> c = new List<Color>();
//        paintStroke.pointColors = c;
//        // List<float> s = new List<float>();
//        paintStroke.pointSizes = new List<float>();

//        for (int i = 0; i < info.verts.Length; i++)
//        {
//            //paintStroke.verts[i] = info.verts[i];  // implicit conversion of SV3 to Vector3
//            paintStroke.verts.Add(info.verts[i]);
//            //Vector4 vector2color = info.pointColors[i]; // implicit conversion of SV4 to Vector4
//            // explicit conversion of SV4 to Vector4
//            //Vector4 vector2color = new Vector4(info.pointColors[i].w, info.pointColors[i].x, info.pointColors[i].y, info.pointColors[i].z);
//            //paintStroke.pointColors.Add(info.pointColors[i]); // implicit conversion of Vector4 to Color
//            Color ptColor = new Color(info.pointColors[i].x, info.pointColors[i].y, info.pointColors[i].z, 1f);
//            paintStroke.pointColors.Add(ptColor);
//            paintStroke.pointSizes.Add(info.pointSizes[i]);

//        }

//        return paintStroke;
//    }


//    public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }


//    public void OnStatusChange(LibPlacenote.MappingStatus prevStatus,
//                                LibPlacenote.MappingStatus currStatus)
//    {
//        //Debug.Log("VERSION?: ");
//        //Debug.Log(mSelectedMapInfo.userData["version"]["a"].ToObject<float>());
//        Debug.Log("prevStatus: " + prevStatus.ToString() +
//                   " currStatus: " + currStatus.ToString());
//        if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
//            prevStatus == LibPlacenote.MappingStatus.LOST)
//        {
//            if (!hasLocalized)
//            {
//                mLabelText.text = "Localized";
//                //LoadShapesJSON(mSelectedMapInfo.metadata.userdata);
//                sModels.LoadFromJSON(mSelectedMapInfo.metadata.userdata);
//                sPaintStrokes.LoadFromJSON(mSelectedMapInfo.metadata.userdata);
//                //LoadPaintStrokesJSON(mSelectedMapInfo.metadata.userdata);
//                hasLocalized = true;
//            }
//        }
//        else if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
//                 prevStatus == LibPlacenote.MappingStatus.WAITING)
//        {
//            mLabelText.text = "Mapping";
//        }
//        else if (currStatus == LibPlacenote.MappingStatus.LOST)
//        {
//            mLabelText.text = "Searching for position lock";
//        }
//        else if (currStatus == LibPlacenote.MappingStatus.WAITING)
//        {
//            if (shapeObjList.Count != 0)
//            {
//                ClearShapes();
//            }
//            OnNewMapClick(); // start session automatically
//        }
//    }

//}


