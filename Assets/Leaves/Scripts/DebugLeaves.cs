using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DebugLeaves : MonoBehaviour {

    public float maxTaperLength = 0.01f;
    public Slider taperSlider;
		
    public void ChangeMaxTaper()
    {
        maxTaperLength = taperSlider.value;
    
	}
}
