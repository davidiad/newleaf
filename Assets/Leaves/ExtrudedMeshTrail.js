import UnityEngine.UI;

// Generates an extrusion trail from the attached mesh
// Uses the MeshExtrusion algorithm in MeshExtrusion.cs to generate and preprocess the mesh.
var time = 2.0;
var autoCalculateOrientation = true;
var minDistance = 0.1;
var invertFaces = false;
private var srcMesh : Mesh;
private var precomputedEdges : MeshExtrusion.Edge[];

// Painting
private var paintOn = false;
//private var paintToggle: GameObject;
//private var paintManagerObject: GameObject;
//private var paintManager;
private var paintOnObject: GameObject;


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
}

public function togglePaint() {
    paintOn = (paintOnObject.GetComponent(PaintOn) as PaintOn).paintOn;
   /* paintOn = !paintOn;
    // let user know that painting is on
    if (paintOn)
    {
        paintToggle.transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        //var b: Button = paintToggle.GetComponent(Button) as Button;
        paintToggle.GetComponent(UI.Button).onClick.AddListener(action);
    }
    else
    {
        paintToggle.transform.localScale = new Vector3(1f, 1f, 1f);
    }*/

}

private var sections = new Array();

function LateUpdate () {
    paintOn = (paintOnObject.GetComponent(PaintOn) as PaintOn).paintOn;
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
	}
	
	// We need at least 2 sections to create the line
	if (sections.length < 2)
		return;

	var worldToLocal = transform.worldToLocalMatrix;
	var finalSections = new Matrix4x4[sections.length];
	var previousRotation : Quaternion;
	
	for (var i=0;i<sections.length;i++)
	{
    
        // explicit declarations as Extruded Trail Sections seems to be required by compiler
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
