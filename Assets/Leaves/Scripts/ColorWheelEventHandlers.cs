using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ColorWheelEventHandlers : MonoBehaviour, IPointerEnterHandler, IDragHandler, IBeginDragHandler, IPointerExitHandler, IDropHandler
{
    Touch touch;

    void Start()
    {
        //touch = new Touch();
        
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("e.pos: " + eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        //if (Input.touchCount > 0)
        //{
            //touch = Input.GetTouch(0);
            this.gameObject.transform.position = eventData.position;  //touch.position;
        //}

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
