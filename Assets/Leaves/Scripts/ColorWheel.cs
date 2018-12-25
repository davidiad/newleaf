using UnityEngine;

public class ColorWheel : MonoBehaviour {

    private float rotZ;
    private RectTransform rectTransform;
	void Start () 
    {
        rectTransform = GetComponent<RectTransform>();
	}
	
	public void ChangeColor () 
    {
        rectTransform.Rotate(0f, 0f, 1f);
        rotZ = rectTransform.rotation.eulerAngles.z;
        //Debug.Log("Color Wheel Rotation: " + rotZ);
	}

    private float CalculateRotationFromDrag()
    {
        Event e = Event.current;
        print("MOUSE POS START");
        Debug.Log(e.mousePosition);
        print("__________END");

        // Get the current center of the colorwheel.
        //float c = rectTransform.
        // Vector2 startVector = Get the screen position of the center. Get the screen pos of start of drag

        return 0f; 

        // 1. Get the vector from local color wheel center to drag start location

        // 2. Each frame while dragging: get the vector from center to drag position

        // 3. Find the difference in angle of these 2 vectors.

        // 4. Rotate that amount

        // 5. Change the color to the corresponding hue

        // 6. ? Need to reset/check for alignment to correct colors periodically?

        //*********Reference************
        // public static float SignedAngle(Vector2 from, Vector2 to);
        // The result is never greater than 180 degrees or smaller than -180 degrees.


    }
}
