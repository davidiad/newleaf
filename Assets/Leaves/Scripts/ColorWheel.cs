using System.Collections;
using System.Collections.Generic;
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
        Debug.Log("Color Wheel Rotation: " + rotZ);

	}
}
