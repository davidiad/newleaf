using System.Collections.Generic;
using UnityEngine;
using System;
using Newtonsoft.Json.Linq;

//[Serializable]
//public class ModelInfo
//{
//    public float px;
//    public float py;
//    public float pz;
//    public float qx;
//    public float qy;
//    public float qz;
//    public float qw;
//    public int modelIndex;
//}

//[Serializable]
//public class ModelInfoArray // changed  name from ModelList, because not a List (analagous to ShapeList)
//{
//    public ModelInfo[] modelInfos;
//}

    public class SerializePaintStrokes : ScriptableObject
{

    // vars are public to allow accessing from LeavesManager
    public String jsonKey;
    public List<ModelInfo> infoList;
    public List<GameObject> objList;

    public void Init()
    {
        infoList = new List<ModelInfo>();
        objList = new List<GameObject>();
        jsonKey = "paintstrokes";
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
        infoList.Add(info);

        // Instantiate and add to scene
        //GameObject model = ModelFromInfo(info);

        // add the game object to object list
        //objList.Add(model);
    }

    // get a custom 3D model
    private PaintStroke ModelFromInfo(ModelInfo info)
    {
        GameObject psHolder = new GameObject("psholder");
        psHolder.AddComponent<PaintStroke>();
        PaintStroke paintStroke = psHolder.GetComponent<PaintStroke>();
        //paintStroke.color = new Color(info.initialColor.x, info.initialColor.y, info.initialColor.z, info.initialColor.w);
        //List<Vector3> v = new List<Vector3>();
        //paintStroke.verts = v;
        //List<Color> c = new List<Color>();
        //paintStroke.pointColors = c;
        //// List<float> s = new List<float>();
        //paintStroke.pointSizes = new List<float>();

        //for (int i = 0; i < info.verts.Length; i++)
        //{
        //    //paintStroke.verts[i] = info.verts[i];  // implicit conversion of SV3 to Vector3
        //    paintStroke.verts.Add(info.verts[i]);
        //    //Vector4 vector2color = info.pointColors[i]; // implicit conversion of SV4 to Vector4
        //    // explicit conversion of SV4 to Vector4
        //    //Vector4 vector2color = new Vector4(info.pointColors[i].w, info.pointColors[i].x, info.pointColors[i].y, info.pointColors[i].z);
        //    //paintStroke.pointColors.Add(info.pointColors[i]); // implicit conversion of Vector4 to Color
        //    Color ptColor = new Color(info.pointColors[i].x, info.pointColors[i].y, info.pointColors[i].z, 1f);
        //    paintStroke.pointColors.Add(ptColor);
        //    paintStroke.pointSizes.Add(info.pointSizes[i]);

        //}

        return paintStroke;
    }

    // convert array of model info to json
    public JObject ToJSON()
    {
        ModelInfoArray modelInfoArray = new ModelInfoArray();
        modelInfoArray.modelInfos = new ModelInfo[infoList.Count];
        for (int i = 0; i < infoList.Count; i++)
        {
            modelInfoArray.modelInfos[i] = infoList[i];
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
                infoList.Add(info);
                //GameObject model = ModelFromInfo(info);
                //objList.Add(model);
            }
        }
    }

    public void ClearModels()
    {
        foreach (var obj in objList)
        {
            Destroy(obj);
        }
        infoList.Clear();
        objList.Clear();
    }

}
