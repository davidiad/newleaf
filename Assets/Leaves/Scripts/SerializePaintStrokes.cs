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
    public List<PaintStroke> objList; // used to hold a list of PaintStrokes recreated from JSON, and then passed to PaintManager's list of paintstrokes

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


    // Helper func to convert info to a PaintStroke object
    private PaintStroke PaintStrokeFromInfo(PaintStrokeInfo info)
    {
        GameObject psHolder = new GameObject("psholder"); // Since PaintStroke is a Monobehavior, it needs a game object to attach to
        psHolder.AddComponent<PaintStroke>();
        PaintStroke paintStroke = psHolder.GetComponent<PaintStroke>();
        paintStroke.color = new Color(info.initialColor.x, info.initialColor.y, info.initialColor.z, info.initialColor.w);
        List<Vector3> v = new List<Vector3>();
        paintStroke.verts = v;
        List<Color> c = new List<Color>();
        paintStroke.pointColors = c;
        paintStroke.pointSizes = new List<float>();

        for (int i = 0; i < info.verts.Length; i++)
        {
            paintStroke.verts.Add(info.verts[i]);
            Color ptColor = new Color(info.pointColors[i].x, info.pointColors[i].y, info.pointColors[i].z, 1f);
            paintStroke.pointColors.Add(ptColor);
            paintStroke.pointSizes.Add(info.pointSizes[i]);
        }

        return paintStroke;
    }

    // convert array of paintstroke info to json
    public JObject ToJSON()
    {
        // Create a new PaintStrokeList with values copied from paintStrokesInfoList(a List of PaintStrokeInfo)
        // Despite the name, PaintStrokeList contains an array (not a List) of PaintStrokeInfo
        // TODO: rename PaintStrokeList to PaintStrokeInfoArray
        // Need this array to convert to a JObject

        PaintStrokeList psList = new PaintStrokeList();
        // define the array
        PaintStrokeInfo[] psiArray = new PaintStrokeInfo[infoList.Count];
        psList.strokes = psiArray;
        // populate the array
        for (int i = 0; i < infoList.Count; i++)
        {
            psiArray[i] = new PaintStrokeInfo();
            psiArray[i].verts = infoList[i].verts;
            psiArray[i].pointColors = infoList[i].pointColors;
            psiArray[i].pointSizes = infoList[i].pointSizes;
            psiArray[i].initialColor = infoList[i].initialColor;
        }

        return JObject.FromObject(psList);
    }

    // reconstitute the JSON
    public void LoadFromJSON(JToken mapMetadata)
    {
        Clear(); // Clear the paintstrokes

        if (mapMetadata is JObject && mapMetadata["paintStrokeList"] is JObject)
        {
            Debug.Log("A-LoadPaintStrokesJSON");
            // this next line breaks when deserializing a list of vector4's
            PaintStrokeList paintStrokes = mapMetadata["paintStrokeList"].ToObject<PaintStrokeList>();
            Debug.Log("B-LoadPaintStrokesJSON");
            if (paintStrokes.strokes == null)
            {
                Debug.Log("no PaintStrokes were added");
                return;
            }

            // (may need to do a for loop to ensure they stay in order?)
            foreach (var paintInfo in paintStrokes.strokes)
            {
                infoList.Add(paintInfo);
                PaintStroke paintstroke = PaintStrokeFromInfo(paintInfo);
                objList.Add(paintstroke); // should be used by PaintManager to recreate painting
                Debug.Log("C-LoadPaintStrokesJSON");
            }
            Debug.Log("D-LoadPaintStrokesJSON");
            paintManager.paintStrokesList = objList; // not really objects, rather components (Monobehaviors)
            Debug.Log("E-LoadPaintStrokesJSON");
            paintManager.RecreatePaintedStrokes();
            Debug.Log("F-LoadPaintStrokesJSON");
        }
    }

    // TODO: practically the same as in SerializeModels, so could be abstracted into a base class
    public void Clear ()
    {
        foreach (var obj in objList)
        {
            Destroy(obj);
        }
        infoList.Clear();
        objList.Clear();
    }

}
