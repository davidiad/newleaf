using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformValues : MonoBehaviour {
    //TODO: Use a scriptable object instead of MonoBehavior
    public Vector3 pos = Vector3.zero; //current position
    public Quaternion rot = Quaternion.identity; //current rotation
    public Vector3 scale = Vector3.one; //current scale

    public void TransferValues(Transform trans)
    {
        pos = trans.localPosition;
        rot = trans.localRotation;
        scale = trans.localScale;
    }
}
