using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// This Class is used as a way to communicate between Javascript and C# scripts.
// After ExtrudedMeshTrail.js is rewritten in C#, it won't be need.
// Meanwhile, don't move this script from the folder it's in, or the project may not work

public class PaintOn : MonoBehaviour {
    public bool paintOn = false;
    public bool meshLoading = false;
    public bool endPainting = false;

    public List<Vector3> currentVertices = new List<Vector3>();

    public void RemoveExtrudeMeshTrailComponent(GameObject go) {
        
    }

	void Start () {
        
	}



}
