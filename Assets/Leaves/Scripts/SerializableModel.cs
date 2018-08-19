using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

[Serializable]
public class ModelInfo
{
    public float px;
    public float py;
    public float pz;
    public float qx;
    public float qy;
    public float qz;
    public float qw;
    public int modelIndex;
}

[Serializable]
public class ModelInfoArray // changed  name from ModelList, because not a List (analagous to ShapeList)
{
    public ModelInfo[] modelInfos;
}

public class SerializableModel : ScriptableObject {

    // vars are public to allow accessing from LeavesManager
    public String jsonKey;// = "models";
    public GameObject[] prefabs; // models to choose from
    //TODO: load the prefabs
    public List<ModelInfo> modelInfoList;// = new List<ModelInfo>(); // need to pass in
    public List<GameObject> modelObjList;// = new List<GameObject>();

    public void Init()
    {
        prefabs = new GameObject[1];
        modelInfoList = new List<ModelInfo>();
        modelObjList = new List<GameObject>();
        jsonKey = "models";
    }

    public void OnAddToScene()
    {
        ModelInfo info = new ModelInfo();

        // get the object transform info to use
        Vector3 pos = Camera.main.transform.position + Camera.main.transform.forward * 1.3f;
        Quaternion rot = Camera.main.transform.rotation;

        // put the transform info into model info object
        info.px = pos.x;
        info.py = pos.y;
        info.pz = pos.z;
        info.qx = rot.x;
        info.qy = rot.y;
        info.qz = rot.z;
        info.qw = rot.w;
        info.modelIndex = 0; // Default to 0 (just one model) for now

        // add info to info list
        modelInfoList.Add(info);

        // Instantiate and add to scene
        GameObject model = ModelFromInfo(info);

        // add the game object to object list
        modelObjList.Add(model);
    }

    // get a custom 3D model
    private GameObject ModelFromInfo(ModelInfo info)
    {
        Vector3 pos = new Vector3(info.px, info.py, info.pz);
        Quaternion rot = new Quaternion(info.qx, info.qy, info.qz, info.qw);
        Vector3 localScale = new Vector3(0.05f, 0.05f, 0.05f);
        GameObject model = Instantiate(prefabs[info.modelIndex], pos, rot);

        return model;
    }

    // convert array of model info to json
    public JObject ToJSON()
    {
        ModelInfoArray modelInfoArray = new ModelInfoArray();
        modelInfoArray.modelInfos = new ModelInfo[modelInfoList.Count];
        for (int i = 0; i < modelInfoList.Count; i++)
        {
            modelInfoArray.modelInfos[i] = modelInfoList[i];
        }

        return JObject.FromObject(modelInfoArray);
    }

    // reconstitute the JSON
    public void LoadFromJSON(JToken mapMetadata)
    {
        ClearModels();

        if (mapMetadata is JObject && mapMetadata[jsonKey] is JObject)
        {
            ModelInfoArray modelInfoArray = mapMetadata[jsonKey].ToObject<ModelInfoArray>();
            if (modelInfoArray.modelInfos == null)
            {
                Debug.Log("No models");
                return;
            }

            // populate the object and info Lists
            foreach (var info in modelInfoArray.modelInfos)
            {
                modelInfoList.Add(info);
                GameObject model = ModelFromInfo(info);
                modelObjList.Add(model);
            }
        }
    }

    public void ClearModels()
    {
        foreach (var obj in modelObjList)
        {
            Destroy(obj);
        }
        modelObjList.Clear();
        modelInfoList.Clear();
    }

}
