import UnityEngine.UI;
import System.Collections.Generic;

// Generates an extrusion trail from the attached mesh
// Uses the MeshExtrusion algorithm in MeshExtrusion.cs to generate and preprocess the mesh.
var time = 2.0;
var autoCalculateOrientation = true;
var minDistance = 0.2;
var invertFaces = false;
private var srcMesh : Mesh;
private var precomputedEdges : MeshExtrusion.Edge[];

// Painting
private var paintOn = false;
private var paintOnObject: GameObject;
private var paintOnComponent: PaintOn;
private var paintVertices : List.<Vector3>;

class ExtrudedTrailSection
{
	var point : Vector3;
	var matrix : Matrix4x4;
	var time : float;
}

function Start ()
{

	srcMesh = GetComponent(MeshFilter).sharedMesh;
	precomputedEdges = MeshExtrusion.BuildManifoldEdges(srcMesh);
    //paintToggle = GameObject.FindWithTag("PaintToggle");
    //paintManagerObject = GameObject.FindWithTag("PaintManager");
    //paintManager = paintManagerObject.GetComponent(PaintManager) as PaintManager;
    paintOnObject = GameObject.FindWithTag("PaintOn");
    paintOnComponent = paintOnObject.GetComponent(PaintOn) as PaintOn;
    paintVertices = new List.<Vector3>();
}

public function togglePaint() {
    paintOn = paintOnComponent.paintOn;
    //paintOn = (paintOnObject.GetComponent(PaintOn) as PaintOn).paintOn;

}

private var sections = new Array();

function LateUpdate () {
    paintOn = paintOnComponent.paintOn;
    if (paintOn) {
	var position = transform.position;
	var now = Time.time;
	/*
	// Remove old sections
	while (sections.length > 0 && now > sections[sections.length - 1].time + time) {
		sections.Pop();
	}
    */


	// Add a new trail section to beginning of array
	if (sections.length == 0 || ((sections[0] as ExtrudedTrailSection).point - position).sqrMagnitude > minDistance * minDistance)
	{
		var section = ExtrudedTrailSection ();
		section.point = position;
		section.matrix = transform.localToWorldMatrix;
		section.time = now;
		sections.Unshift(section);
        paintOnComponent.currentVertices.Add(position);
        //paintVertices.Add(position);
	}
	
	// We need at least 2 sections to create the line
	if (sections.length < 2)
		return;

	var worldToLocal = transform.worldToLocalMatrix;
	var finalSections = new Matrix4x4[sections.length];
	var previousRotation : Quaternion;
	
	for (var i=0;i<sections.length;i++)
	{
    
        // explicit declarations as Extruded Trail Sections required by compiler
        var s0: ExtrudedTrailSection = sections[0];
        var s1: ExtrudedTrailSection = sections[1];
        var si: ExtrudedTrailSection = sections[i]; 



		if (autoCalculateOrientation)
		{
			if (i == 0)
			{
				var direction = s0.point - s1.point;
				var rotation = Quaternion.LookRotation(direction, Vector3.up);
				previousRotation = rotation;
				finalSections[i] = worldToLocal * Matrix4x4.TRS(position, rotation, Vector3.one);	
			}
			// all elements get the direction by looking up the next section
			else if (i != sections.length - 1)
			{	
                var siPlus: ExtrudedTrailSection = sections[i+1];
				direction = si.point - siPlus.point;
				rotation = Quaternion.LookRotation(direction, Vector3.up);
				
				// When the angle of the rotation compared to the last segment is too high
				// smooth the rotation a little bit. Optimally we would smooth the entire sections array.
				if (Quaternion.Angle (previousRotation, rotation) > 20)
					rotation = Quaternion.Slerp(previousRotation, rotation, 0.5);
					
				previousRotation = rotation;
				finalSections[i] = worldToLocal * Matrix4x4.TRS(si.point, rotation, Vector3.one);
			}
			// except the last one, which just copies the previous one
			else
			{
				finalSections[i] = finalSections[i-1];
			}
		}
		else
		{
			if (i == 0)
			{
				finalSections[i] = Matrix4x4.identity;
			}
			else
			{
				finalSections[i] = worldToLocal * si.matrix;
			}
		}
	}
	
	// Rebuild the extrusion mesh	
	MeshExtrusion.ExtrudeMesh (srcMesh, GetComponent(MeshFilter).mesh, finalSections, precomputedEdges, invertFaces);
    }
}

@script RequireComponent (MeshFilter)
