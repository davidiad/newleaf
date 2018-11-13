using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO: should be a UI element
public class DownloadBar : MonoBehaviour
{
    public GameObject prefab;
    private GameObject bar;
    public float progress { get; set; }
    public bool finished { get; set;  }
    public void UpdateDisplay () 
    {
        
    }
}

public class DownloadManager : MonoBehaviour {


}
