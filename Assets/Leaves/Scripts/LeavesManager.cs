using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;
using UnityEngine.XR.iOS;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

public class LeavesManager : MonoBehaviour, PlacenoteListener // Updated to Placenote 1.6.8
{
    public GameObject modelPrefab;
    public Vector3 paintPosition;
    [SerializeField] Material mShapeMaterial;

    // Get refs to buttons in the UI
    private GameObject mMapLoader;
    private GameObject mExitButton;
    private RectTransform mListContentParent;
    private ToggleGroup mToggleGroup; // Toggle Group for the Toggles in each list element menu item
    private GameObject mPlaneDetectionToggle;
    private Text mLabelText;
    private Text uploadText;
    private GameObject mapButton;
    private Text nameHolder;
    private InputField nameInput;
    private PlacenoteARGeneratePlane mPNPlaneManager;

    private UnityARSessionNativeInterface mSession;
    private bool mARKitInit = false;
    //    private List<ShapeInfo> shapeInfoList = new List<ShapeInfo>();
    private List<GameObject> shapeObjList = new List<GameObject>();
    private List<PaintStrokeInfo> paintStrokeInfoList = new List<PaintStrokeInfo>();
    private List<PaintStroke> paintStrokeObjList = new List<PaintStroke>();

    private PaintManager paintManager;
    private SerializeModels sModels;
    private SerializePaintStrokes sPaintStrokes;
    private SerializePeople sPeople;

    private Slider mRadiusSlider;
    private float defaultDistance = 8000f;
    private LibPlacenote.MapMetadataSettable mCurrMapDetails;
    private bool mReportDebug = false;
    private string mSaveMapId = null;

    private LibPlacenote.MapInfo mSelectedMapInfo;

    private bool hasLocalized; // flag to prevent continually reloading the metadata when position is lost and regained
    private bool mappingStarted;
    private string currentMapId;

    private string currentName;

    private Coroutine pulseMapButton;

    private string mSelectedMapId
    {
        get
        {
            return
            mSelectedMapInfo != null ? mSelectedMapInfo.placeId : null;
        }
    }

    bool ARPlanePaintingStatus;

    void Start()
    {
        // Set up SerializableModel's
        sModels = ScriptableObject.CreateInstance<SerializeModels>();
        sModels.Init();
        sModels.prefabs[0] = modelPrefab;

        sPaintStrokes = ScriptableObject.CreateInstance<SerializePaintStrokes>();
        sPaintStrokes.Init();

        sPeople = ScriptableObject.CreateInstance<SerializePeople>();
        sPeople.Init();

        InitUI();

        currentMapId = "";
        mappingStarted = false;
        hasLocalized = false;
        mPNPlaneManager = GameObject.FindWithTag("PNPlaneManager").GetComponent<PlacenoteARGeneratePlane>();

        Input.location.Start();

        mSession = UnityARSessionNativeInterface.GetARSessionNativeInterface();
        StartARKit();
        FeaturesVisualizer.EnablePointcloud();
        LibPlacenote.Instance.RegisterListener(this);

        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
        ARPlanePaintingStatus = mPlaneDetectionToggle.GetComponent<Toggle>().isOn;
        paintManager.ARPlanePainting = ARPlanePaintingStatus;
        paintManager.paintOnTouch = !ARPlanePaintingStatus; // TODO: make an enum to replace multiple bools

    }

    private void InitUI()
    {
        mMapLoader = GameObject.FindWithTag("MapLoader");
        mExitButton = GameObject.FindWithTag("ExitMapButton");

        GameObject ListParent = GameObject.FindWithTag("ListContentParent");
        mListContentParent = ListParent.GetComponent<RectTransform>();
        mToggleGroup = ListParent.GetComponent<ToggleGroup>();

        mPlaneDetectionToggle = GameObject.FindWithTag("PlaneDetectionToggle");
        mLabelText = GameObject.FindWithTag("LabelText").GetComponent<Text>();
        uploadText = GameObject.FindWithTag("UploadText").GetComponent<Text>();
        mapButton = GameObject.FindWithTag("MapButton");
        mRadiusSlider = GameObject.FindWithTag("RadiusSlider").GetComponent<Slider>();
        nameHolder = GameObject.FindWithTag("name").GetComponent<Text>();
        nameInput = GameObject.FindWithTag("nameInput").GetComponent<InputField>();
        ResetSlider();
        mMapLoader.SetActive(false); // needs to be active at Start, so the reference to it can be found
      
        LoadLocalData();
        //UpdateName();

    }

    //TODO: UpdateName is both here and in SerializePeople. Consolidate to one.
    public void UpdateName()
    {
        string n = nameHolder.text;
        if (n != null)
        {
            currentName = n;
            nameInput.text = currentName;
            SaveLocalData();
            sPeople.currentName = currentName;
        } 
        sPeople.OnNameChange();
    }

    // force Ahead Of Time compiling of List<SerializableVector3> with this unused method
    // Fixes error in deserializing List of SerializableVector3
    // (alternative fix -- could define SerializableVector3 as class instead of struct)
    private void dummyMethod()
    {
        List<SerializableVector3> forceAOT = new List<SerializableVector3>();
    }

    public void LoadLocalData()
    {
        Debug.Log("LLD");
        if (PlayerPrefs.HasKey("name"))
        {
            
            currentName = PlayerPrefs.GetString("name");
            Debug.Log(currentName);
            nameHolder.text = currentName;
            nameInput.text = currentName; //TODO: need both nameHolder and nameInput? (nameInput seems to supercede) 
        }
    }

    public void SaveLocalData()
    {
        PlayerPrefs.SetString("name", currentName);
        PlayerPrefs.Save();
    }

    void Update()
    {
        if (!mappingStarted) // start mapping automatically
        {
            OnNewMapClick();
        }
        //if (mFrameUpdated)
        //{
        //    mFrameUpdated = false;
        //    if (mImage == null)
        //    {
        //        InitARFrameBuffer();
        //    }

        //    if (mARCamera.trackingState == ARTrackingState.ARTrackingStateNotAvailable)
        //    {
        //        // ARKit pose is not yet initialized
        //        return;
        //    }
        //    else if (!mARKitInit)
        //    {
        //        mARKitInit = true;
        //        mLabelText.text = "ARKit Initialized";
        //        if (!LibPlacenote.Instance.Initialized())
        //        {
        //            Debug.Log("initialized Mapping");


        //        }

        //    }

        //    Matrix4x4 matrix = mSession.GetCameraPose();

        //    Vector3 arkitPosition = PNUtility.MatrixOps.GetPosition(matrix);
        //    Quaternion arkitQuat = PNUtility.MatrixOps.GetRotation(matrix);

        //    LibPlacenote.Instance.SendARFrame(mImage, arkitPosition, arkitQuat,
        //                                      mARCamera.videoParams.screenOrientation);

        //}

        paintPosition = Camera.main.transform.position + Camera.main.transform.forward * 0.3f;
        if (mappingStarted)
        {
            pulseMapButton = StartCoroutine(PulseColor(mapButton.GetComponent<Image>()));
        }
    }

    private void ActivateMapButton(bool mappingOn)
    {
        Image imageComponent = mapButton.GetComponent<Image>();
        imageComponent.fillCenter = mappingOn;
        if (mappingOn)
        {
            mapButton.GetComponent<CanvasGroup>().alpha = 1.0f;
            imageComponent.color = Color.yellow;
            //pulseMapButton = StartCoroutine(PulseColor(imageComponent));
        }
        else
        {
            mapButton.GetComponent<CanvasGroup>().alpha = 0.4f;
            StopCoroutine(pulseMapButton);
            imageComponent.color = Color.white;
        }
    }

    // Make a color pulsate
    private IEnumerator PulseColor(Image img)
    {
        float alpha = (Mathf.Sin(Time.time * 2f) + 1.9f) * 0.33f;
        img.color = new Color(1.0f, 0.95f, 0.6f, alpha);

        yield return null;
    }

    //TODO: Use search map instead (ListMap downloads the entire map + metadata for all maps, very inefficient)
    public void OnListMapClick()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
            return;
        }

        UpdateName();
        OnRadiusSelect();

        /*
        foreach (Transform t in mListContentParent.transform)
        {
            // TODO: check if this destroy com mand is a cause of the slow loading of the list
            Destroy(t.gameObject);
        }
        mMapLoader.SetActive(true);
        //mMapListPanel.SetActive(true);

        //        mInitButtonPanel.SetActive(false); // added in 1.62

        //        mRadiusSlider.gameObject.SetActive(true);

        LibPlacenote.Instance.ListMaps((mapList) =>
        {
            //Debug.Log("MAPID INFO'S HOW MANY: " + mapList[0].metadata.ToString()); 
            // render the map list!
            foreach (LibPlacenote.MapInfo mapId in mapList)
            {

                if (mapId.metadata != null) // extra if, can be removed, prevent editor warning
                {
                    if (mapId.metadata.userdata != null)
                    {
                        //Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
                    }
                    AddMapToList(mapId);
                }
            }
        });
        */
    }

    public void SearchUserData()
    {
        string q = "people[**][name=" + currentName + "]";
        /* JSON snippet
          "people": { "personInfos": [ { "ID": 0, "name": "Kelly", "role": "Sender" } ]
        */
        LibPlacenote.Instance.SearchMapsByUserData(q, (mapList) =>
        {
            Debug.Log("MAPLIST: " + mapList);
            //TODO:// extract following to method
            foreach (Transform t in mListContentParent.transform)
            {
                Destroy(t.gameObject);
            }
            // render the map list!
            foreach (LibPlacenote.MapInfo mapId in mapList)
            {
                Debug.Log(mapId);
                if (mapId.metadata != null) // extra if statement just prevents warning in Editor
                {
                    if (mapId.metadata.userdata != null)
                    {
                        Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
                    }
                    AddMapToList(mapId);
                }
            }
        });
    }

    // Radius stuff - new with PN 1.62
    public void OnRadiusSelect()
    {
        LocationInfo locationInfo = Input.location.lastData;

        Debug.Log(locationInfo.ToString());


        float radiusSearch = 20f;//mRadiusSlider.value;// * mMaxRadiusSearch;
        //mRadiusLabel.text = "Distance Filter: " + (radiusSearch / 1000.0).ToString("F2") + " km";
        Debug.Log(radiusSearch.ToString());
        mMapLoader.SetActive(true);
        SearchUserData();
        //LibPlacenote.Instance.SearchMaps(locationInfo.latitude, locationInfo.longitude, radiusSearch,
        //(mapList) =>
        //{
        //    //                Debug.Log("MAPID INFO'S HOW much: " + mapList[0].metadata.ToString()); 

        //    foreach (Transform t in mListContentParent.transform)
        //    {
        //        Destroy(t.gameObject);
        //    }
        //    // render the map list!
        //    foreach (LibPlacenote.MapInfo mapId in mapList)
        //    {
        //        Debug.Log(mapId);
        //        if (mapId.metadata != null) // extra if statement just prevents warning in Editor
        //        {
        //            if (mapId.metadata.userdata != null)
        //            {
        //                Debug.Log(mapId.metadata.userdata.ToString(Formatting.None));
        //            }
        //            AddMapToList(mapId);
        //        }
        //    }
        //});
    }

    public void ResetSlider()
    {
        mRadiusSlider.value = defaultDistance;
        //mRadiusLabel.text = "Distance Filter: Off";
    }


    public void OnCancelClick()
    {
        mMapLoader.SetActive(false);
        ResetSlider();
    }


    public void OnExitClick()
    {
        paintManager.Reset();
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
        // GameObject newElement = Instantiate(mListElement) as GameObject;
        GameObject newElement = Instantiate(Resources.Load("LeafInfoElement", typeof(GameObject))) as GameObject;
        MapInfoElement listElement = newElement.GetComponent<MapInfoElement>();
        listElement.Initialize(mapInfo, mToggleGroup, mListContentParent, (value) =>
        {
            OnMapSelected(mapInfo);
        });
    }

    void OnMapSelected(LibPlacenote.MapInfo mapInfo)
    {
        Debug.Log("SELECT: " + mapInfo.metadata.userdata);
        mSelectedMapInfo = mapInfo;
        LoadFromMetadata();
        mMapLoader.SetActive(true);
        mRadiusSlider.gameObject.SetActive(false);
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
            return;
        }

        hasLocalized = false;
        mLabelText.text = "Loading Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.LoadMap(mSelectedMapId,
            (completed, faulted, percentage) =>
            {
                if (completed)
                {
                    mMapLoader.SetActive(false);

                    LibPlacenote.Instance.StartSession();
                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
                    currentMapId = mSelectedMapId;

                    if (mReportDebug)
                    {
                        LibPlacenote.Instance.StartRecordDataset(
                            (datasetCompleted, datasetFaulted, datasetPercentage) =>
                            {

                                if (datasetCompleted)
                                {
                                    mLabelText.text = "Dataset Upload Complete";
                                }
                                else if (datasetFaulted)
                                {
                                    mLabelText.text = "Dataset Upload Faulted";
                                }
                                else
                                {
                                    mLabelText.text = "Dataset Upload: " + datasetPercentage.ToString("F2") + "/1.0";
                                }
                            });
                        Debug.Log("Started Debug Report");
                    }
                    mLabelText.text = "Loaded ID: " + mSelectedMapId;
                }
                else if (faulted)
                {
                    mLabelText.text = "Failed to load ID: " + mSelectedMapId;
                }
                else
                {
                    mLabelText.text = "Map Download: " + percentage.ToString("F2") + "/1.0";
                }
            }
        );
    }

    public void OnDeleteMapClicked()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
            return;
        }

        mLabelText.text = "Deleting Map ID: " + mSelectedMapId;
        LibPlacenote.Instance.DeleteMap(mSelectedMapId, (deleted, errMsg) =>
        {
            if (deleted)
            {
                //mMapSelectedPanel.SetActive(false);
                mLabelText.text = "Deleted ID: " + mSelectedMapId;
                OnListMapClick();
            }
            else
            {
                mLabelText.text = "Failed to delete ID: " + mSelectedMapId;
            }
        });
    }


    public void OnNewMapClick()
    {
        ConfigureSession(false);

        if (LibPlacenote.Instance.Initialized())
        {
            if (!mappingStarted)
            {
                GameObject.FindWithTag("MapButton").GetComponent<CanvasGroup>().alpha = 1.0f;
                //mMappingButtonPanel.SetActive(true);
                mPlaneDetectionToggle.SetActive(true);
                Debug.Log("Started Session");
                mappingStarted = true;
                ActivateMapButton(mappingStarted);
                LibPlacenote.Instance.StartSession();
            }

            if (mReportDebug)
            {
                LibPlacenote.Instance.StartRecordDataset(
                    (completed, faulted, percentage) =>
                    {
                        if (completed)
                        {
                            mLabelText.text = "Dataset Upload Complete";
                        }
                        else if (faulted)
                        {
                            mLabelText.text = "Dataset Upload Faulted";
                        }
                        else
                        {
                            mLabelText.text = "Dataset Upload: (" + percentage.ToString("F2") + "/1.0)";
                        }
                    });
                Debug.Log("Started Debug Report");
            }
        }
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
//#if !UNITY_EDITOR
        ARKitWorldTrackingSessionConfiguration config = new ARKitWorldTrackingSessionConfiguration();

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
//#endif
    }


    public void OnSaveMapClick()
    {
        if (!LibPlacenote.Instance.Initialized())
        {
            Debug.Log("SDK not yet initialized");
//            ToastManager.ShowToast("SDK not yet initialized", 2f);
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

                    //clear all existing planes
                    mPNPlaneManager.ClearPlanes();
                    mPlaneDetectionToggle.GetComponent<Toggle>().isOn = false;

                    // Updated for 1.62
                    LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();
                    metadata.name = RandomName.Get();
                    mLabelText.text = "Saved Map Name: " + metadata.name;
                    JObject userdata = new JObject();
                    metadata.userdata = userdata;

                    userdata[sModels.jsonKey] = sModels.ToJSON(); // replaces shapeList

                    userdata[sPaintStrokes.jsonKey] = sPaintStrokes.ToJSON();

                    //userdata["person"] = name;
                    userdata[sPeople.jsonKey] = sPeople.ToJSON();

                    if (useLocation)
                    {
                        metadata.location = new LibPlacenote.MapLocation();
                        metadata.location.latitude = locationInfo.latitude;
                        metadata.location.longitude = locationInfo.longitude;
                        metadata.location.altitude = locationInfo.altitude;
                    }
                    else
                    { // default location so that JSON object is not invalid due to missing location data
                        metadata.location = new LibPlacenote.MapLocation();
                        metadata.location.latitude = 50f;
                        metadata.location.longitude = 100f;
                        metadata.location.altitude = 10f;
                    }
                    LibPlacenote.Instance.SetMetadata(mapId, metadata);
                    currentMapId = mCurrMapDetails.name; //mapId;  // from prev. PN version, still needed?
                    mCurrMapDetails = metadata;
                },
                (completed, faulted, percentage) =>
                { // progressCb  upon transfer to cloud
                    String percentText = (percentage * 100f).ToString();
                    uploadText.text = "Map upload status– Completed: " + completed + "    Faulted: " + faulted + "   " + "\n" + percentText + "% uploaded";

                    ActivateMapButton(false);
                }
            );
        }
    }

    public void OnClickUpdate()
    {
        if (currentMapId != "")
        {
            Debug.Log(currentMapId);
            SetMetaData(currentMapId);
        }
    }

    //TODO: update for 1.62
    private void SetMetaData(string mid)
    {
        sPaintStrokes.OnAddToScene();

        bool useLocation = Input.location.status == LocationServiceStatus.Running;
        LocationInfo locationInfo = Input.location.lastData;

        // Update for PN 1.62
        LibPlacenote.MapMetadataSettable metadata = new LibPlacenote.MapMetadataSettable();
        metadata.name = RandomName.Get();
        mLabelText.text = "Saved Map Name: " + metadata.name;
        JObject userdata = new JObject();
        metadata.userdata = userdata;

        //JObject shapeList = Shapes2JSON();
        //userdata["shapeList"] = shapeList;

        userdata[sModels.jsonKey] = sModels.ToJSON();
        userdata[sPaintStrokes.jsonKey] = sPaintStrokes.ToJSON();
        userdata[sPeople.jsonKey] = sPeople.ToJSON();


        if (useLocation)
        {
            metadata.location = new LibPlacenote.MapLocation();
            metadata.location.latitude = locationInfo.latitude;
            metadata.location.longitude = locationInfo.longitude;
            metadata.location.altitude = locationInfo.altitude;
        }
        else
        { // default location so that JSON object is not invalid due to missing location data
            metadata.location = new LibPlacenote.MapLocation();
            metadata.location.latitude = 50f;
            metadata.location.longitude = 100f;
            metadata.location.altitude = 10f;
        }

        LibPlacenote.Instance.SetMetadata(mid, metadata);
    }

    public void OnAddPersonEvent()
    {
        sPeople.OnNameChange();
    }

    public void OnDropShapeClick()
    {
        sModels.OnAddToScene();
    }

    public void OnDropPaintStrokeClick()  // called when SaveMap is clicked. Add all the paint strokes to the lists at once
    {
        sPaintStrokes.OnAddToScene();
    }

    //private GameObject ShapeFromInfo(ShapeInfo info)
    //{
    //    GameObject shape = GameObject.CreatePrimitive((PrimitiveType)info.shapeType);
    //    shape.transform.position = new Vector3(info.px, info.py, info.pz);
    //    shape.transform.rotation = new Quaternion(info.qx, info.qy, info.qz, info.qw);
    //    shape.transform.localScale = new Vector3(0.05f, 0.05f, 0.05f);
    //    shape.GetComponent<MeshRenderer>().material = mShapeMaterial;

    //    return shape;
    //}

    //private void ClearShapes()
    //{
    //    foreach (var obj in shapeObjList)
    //    {
    //        Destroy(obj);
    //    }
    //    shapeObjList.Clear();
    //    shapeInfoList.Clear();
    //}


    public void OnPose(Matrix4x4 outputPose, Matrix4x4 arkitPose) { }


    public void OnStatusChange(LibPlacenote.MappingStatus prevStatus,
                                LibPlacenote.MappingStatus currStatus)
    {
        Debug.Log("prevStatus: " + prevStatus.ToString() +
                   " currStatus: " + currStatus.ToString());
        if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
            prevStatus == LibPlacenote.MappingStatus.LOST)
        {
            if (!hasLocalized)
            {
                mLabelText.text = "Localized";
                LoadFromMetadata();
                hasLocalized = true;
            }
        }
        else if (currStatus == LibPlacenote.MappingStatus.RUNNING &&
                 prevStatus == LibPlacenote.MappingStatus.WAITING)
        {
            mLabelText.text = "Mapping";
            if (!hasLocalized && (mSelectedMapInfo != null))
            {
                mLabelText.text = "Localized";
                LoadFromMetadata();
                hasLocalized = true;
            }
        }
        else if (currStatus == LibPlacenote.MappingStatus.LOST)
        {
            mLabelText.text = "Searching for position lock";
        }
        else if (currStatus == LibPlacenote.MappingStatus.WAITING)
        {
            if (shapeObjList.Count != 0)
            {
                sModels.Clear();
            }
            OnNewMapClick(); // start session automatically
        }
    }

    private void LoadFromMetadata()
    {
        sModels.LoadFromJSON(mSelectedMapInfo.metadata.userdata);
        sPaintStrokes.LoadFromJSON(mSelectedMapInfo.metadata.userdata);
    }
}