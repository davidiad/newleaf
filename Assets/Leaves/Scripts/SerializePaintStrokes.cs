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

[Serializable]
public class PaintStrokeInfo
{
    public SerializableVector3[] verts;
    public SerializableVector3[] pointColors; // alpha is always 1, and using V3 avoids deserialization problems with V4
    public float[] pointSizes;
    public SerializableVector4 initialColor; // initial color of stroke. Color implicitly converts to Vector4.
}

[Serializable]
public class PaintStrokeList // TODO: rename to PaintStrokeInfoArray
{
    public PaintStrokeInfo[] strokes; // TODO: rename to paintStrokeInfos
}



public class SerializePaintStrokes : ScriptableObject
{

    // vars are public to allow accessing from LeavesManager
    public String jsonKey;
    public List<PaintStrokeInfo> infoList;
    public List<PaintStroke> objList;

    private PaintManager paintManager;

    public void Init()
    {
        infoList = new List<PaintStrokeInfo>();
        objList = new List<PaintStroke>();
        jsonKey = "paintstrokes";

        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
    }

    public void OnAddToScene()// called when SaveMap is clicked. Add all the paint strokes to the lists at once
    {
        Debug.Log("1-OnDropPaintStrokeClick");
        objList = paintManager.paintStrokesList;
        Debug.Log("2-OnDropPaintStrokeClick");
        // for each PaintStroke, convert to a PaintStrokeInfo, and add to paintStrokesInfoList
        if (objList.Count > 0)
        {
            Debug.Log("3-OnDropPaintStrokeClick");
            foreach (var ps in objList) // TODO: convert to for loop (?)
            {
                // Add the intialColor of the paintstroke
                Vector4 c = ps.color; // implicit conversion of Color to Vector4
                Debug.Log("4-OnDropPaintStrokeClick: " + c.x + " | " + c.y + " | " + c.z + " | " + c.w);
                PaintStrokeInfo psi = new PaintStrokeInfo();
                psi.initialColor = c; // implicit conversion of Vector4 to SerialiazableVector4  

                // Add the verts
                int vertCount = ps.verts.Count;
                //todo: combine in 1 line?
                SerializableVector3[] psiverts = new SerializableVector3[vertCount];
                psi.verts = psiverts;

                // Add the colors per point
                SerializableVector3[] psicolors = new SerializableVector3[vertCount];
                psi.pointColors = psicolors;
                Debug.Log("psi.pointColors length: " + psi.pointColors.Length);

                // Add the size per point
                psi.pointSizes = new float[vertCount];


                if (vertCount > 0)
                {
                    Debug.Log("5-OnDropPaintStrokeClick");
                    for (int j = 0; j < vertCount; j++)
                    {
                        Debug.Log("6-OnDropPaintStrokeClick and ps.verts.Count is: " + ps.verts.Count);
                        //psi.verts[j] = new SerializableVector3(ps.verts[j].x, ps.verts[j].y, ps.verts[j].z);

                        psi.verts[j] = ps.verts[j]; // auto-conversion sv3 and Vector3
                        Debug.Log("6.5-OnDropPaintStrokeClick");
                        //Vector4 vector4color = ps.pointColors[j]; // implicit conversion of Color to Vector4
                        psi.pointColors[j] = new Vector3(ps.pointColors[j].r, ps.pointColors[j].g, ps.pointColors[j].b);
                        psi.pointSizes[j] = ps.pointSizes[j];
                    }
                    Debug.Log("7-OnDropPaintStrokeClick");
                    infoList.Add(psi);
                    Debug.Log("8-OnDropPaintStrokeClick");
                }
            }
        }
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
 //   public JObject ToJSON()
 //   {
        //ModelInfoArray modelInfoArray = new ModelInfoArray();
        //modelInfoArray.modelInfos = new ModelInfo[infoList.Count];
        //for (int i = 0; i < infoList.Count; i++)
        //{
        //    modelInfoArray.modelInfos[i] = infoList[i];
        //}

        //return JObject.FromObject(modelInfoArray);
 //   }

    // reconstitute the JSON
    public void LoadFromJSON(JToken mapMetadata)
    {
        //ClearModels();

        //if (mapMetadata is JObject && mapMetadata[jsonKey] is JObject)
        //{
        //    ModelInfoArray modelInfoArray = mapMetadata[jsonKey].ToObject<ModelInfoArray>();
        //    if (modelInfoArray.modelInfos == null)
        //    {
        //        Debug.Log("No models");
        //        return;
        //    }

        //    // populate the object and info Lists
        //    foreach (var info in modelInfoArray.modelInfos)
        //    {
        //        infoList.Add(info);
        //        //GameObject model = ModelFromInfo(info);
        //        //objList.Add(model);
        //    }
        //}
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
