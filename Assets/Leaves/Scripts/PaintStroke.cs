using System.Collections.Generic;
using UnityEngine;

// TODO:(?) Should this be a struct? (or Scriptable object?)
public class PaintStroke : MonoBehaviour
{
    public List<Vector3> verts; // TODO: rename to points, or positions, so as not to confuse with verts of the generated trail mesh
    public List<Color> pointColors; // will hold colors of individual points
    public List<float> pointSizes; // will hold size of individual points
    public Color color; // initial color of stroke (and default color if no point color)
}