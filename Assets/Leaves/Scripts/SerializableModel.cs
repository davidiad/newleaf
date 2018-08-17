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
public class ModelList
{
    public ModelInfo[] models;
}


public class SerializableModel : ScriptableObject {

    public GameObject[] prefabs;
    //TODO: load the prefabs
    
    private JObject toJSON(List<ModelInfo> modelInfoList)
    {
        ModelList modelList = new ModelList();
        modelList.models = new ModelInfo[modelInfoList.Count];
        for (int i = 0; i < modelInfoList.Count; i++)
        {
            modelList.models[i] = modelInfoList[i];
        }

        return JObject.FromObject(modelList);
    }

    // Add a custom 3D model
    private GameObject ModelFromInfo(ModelInfo info)
    {
        Vector3 pos = new Vector3(info.px, info.py, info.pz);
        Quaternion rot = new Quaternion(info.qx, info.qy, info.qz, info.qw);
        Vector3 localScale = new Vector3(0.05f, 0.05f, 0.05f);
        GameObject model = Instantiate(prefabs[info.modelIndex], pos, rot);

        return model;
    }

}
