using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelEventHandlers : MonoBehaviour, IPointerEnterHandler, IDragHandler, IBeginDragHandler, IPointerExitHandler, IDropHandler
{
    private PaintManager paintManager;
    private RectTransform rectTransform;
    private Vector3 wheelPos;
    private Vector3 previousDir;
    private float rotZ = 0.0f;
    private float angleChange = 0.0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();     
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wheelPos = this.gameObject.transform.position;
        previousDir = new Vector3(eventData.position.x, eventData.position.y, wheelPos.z) - wheelPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, wheelPos.z);//cam.nearClipPlane);
        Vector3 dir = pos - wheelPos;
        Debug.DrawRay(wheelPos, dir, Color.red, 0.2f);

        // Compare current angle with previous angle and rotate
        angleChange = Vector3.SignedAngle(previousDir, dir, Camera.main.transform.forward);
        rectTransform.Rotate(0f, 0f, angleChange);
        // make current dir the previousDir in prep for next frame
        previousDir = dir;

        // convert rotation to range of 0...1 for hue
        rotZ = rectTransform.rotation.eulerAngles.z;
        float newHue = 1 - (rotZ % 360.0f) / 360.0f;
        paintManager.AdjustHue(newHue);
    }

    public void OnDrop(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //throw new System.NotImplementedException();
    }

}
