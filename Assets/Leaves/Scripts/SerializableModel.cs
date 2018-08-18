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
public class ModelInfoArray // change  name, because not a List
{
    public ModelInfo[] modelInfos;
}

public class SerializableModel : ScriptableObject {

    public GameObject[] prefabs; // models to choose from
    //TODO: load the prefabs
    private List<ModelInfo> modelInfoList = new List<ModelInfo>(); // need to pass in
    private List<GameObject> modelObjList = new List<GameObject>();

    // convert array of model info to json
    private JObject ToJSON()
    {
        ModelInfoArray modelInfoArray = new ModelInfoArray();
        modelInfoArray.modelInfos = new ModelInfo[modelInfoList.Count];
        for (int i = 0; i < modelInfoList.Count; i++)
        {
            modelInfoArray.modelInfos[i] = modelInfoList[i];
        }

        return JObject.FromObject(modelInfoArray);
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

    public void OnAddToScene(ModelInfo info)
    {
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
        info.modelIndex = 0; // modelIndex should already be in the info parameter

        // add info to info list
        modelInfoList.Add(info);

        // Instantiate and add to scene
        GameObject model = ModelFromInfo(info);

        // add the game object to object list
        modelObjList.Add(model);
    }

}
