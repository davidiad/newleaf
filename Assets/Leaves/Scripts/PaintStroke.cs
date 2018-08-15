using System.Collections.Generic;
using UnityEngine;

// TODO:(?) Should this be a struct? (or Scriptable object?)
public class PaintStroke : MonoBehaviour
{
    //public int ID { get; set; }
    //public string SomethingWithText { get; set; }
    public List<Vector3> verts;// { get; set; }
    public List<Color> pointColors; // will hold colors of individual points
    public List<float> pointSizes; // will hold size of individual points
    public Color color;// { get; set; } // initial color of stroke (and default color if no point color)
}