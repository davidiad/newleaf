using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelEventHandlers : MonoBehaviour, IPointerEnterHandler, IDragHandler, IBeginDragHandler, IPointerExitHandler, IDropHandler
{
    //Touch touch;
    private PaintManager paintManager;
    private RectTransform rectTransform;
    private Vector3 wheelPos;
    private float rotZ;
    private Vector3 previousDir;
    private float angleChange = 0.0f;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
        //touch = new Touch();
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        wheelPos = this.gameObject.transform.position;
        previousDir = new Vector3(eventData.position.x, eventData.position.y, wheelPos.z) - wheelPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (Input.touchCount > 0)
        //{
            //touch = Input.GetTouch(0);
            //this.gameObject.transform.position = eventData.position;  //touch.position;
        //}
        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, wheelPos.z);//cam.nearClipPlane);
        Vector3 dir = pos - wheelPos;
        Debug.DrawRay(wheelPos, dir, Color.red, 0.2f);

        // do stuff with prev
        angleChange = Vector3.SignedAngle(previousDir, dir, Camera.main.transform.forward);
        rectTransform.Rotate(0f, 0f, angleChange);

        // make current dir the previousDir for next frame
        previousDir = dir;

        // Get the rotation z value, and convert to range of 0...1 for hue

        // Send new hue value to paintmanager to change the color
        paintManager.AdjustHue(0f);

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
