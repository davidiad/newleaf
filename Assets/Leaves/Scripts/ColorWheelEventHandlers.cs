using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelEventHandlers : MonoBehaviour, IPointerEnterHandler, IDragHandler, IBeginDragHandler, IPointerExitHandler, IDropHandler
{
    //Touch touch;

    private Camera cam;
    private Vector3 wheelPos;
    private float rotZ;
    private RectTransform rectTransform;

    void Start()
    {
        cam = Camera.main;
        rectTransform = GetComponent<RectTransform>();

        //touch = new Touch();
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("e.pos: " + eventData.position);
        wheelPos = this.gameObject.transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (Input.touchCount > 0)
        //{
            //touch = Input.GetTouch(0);
            //this.gameObject.transform.position = eventData.position;  //touch.position;
        //}
        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, cam.nearClipPlane);
        //Vector3 p = cam.ScreenToWorldPoint(pos);
        Vector3 dir = pos - wheelPos;
        //Debug.DrawRay(wheelPos, pos.normalized * 200f, Color.red, 0.2f);
        Debug.DrawRay(wheelPos, dir, Color.red, 0.2f);

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
