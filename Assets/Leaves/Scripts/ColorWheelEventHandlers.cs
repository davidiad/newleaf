using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelEventHandlers : MonoBehaviour, IPointerEnterHandler, IDragHandler, IBeginDragHandler, IPointerExitHandler, IDropHandler
{
    //Touch touch;

    private Vector3 wheelPos;
    private float rotZ;
    private RectTransform rectTransform;

    void Start()
    {
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
        Vector3 pos = new Vector3(eventData.position.x, eventData.position.y, wheelPos.z);
        Debug.DrawRay(wheelPos, pos.normalized * 200f, Color.red, 0.2f);
        Debug.Log("e.pos: " + eventData.position);
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
