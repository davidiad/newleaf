//Attach this script to your Canvas GameObject.
//Also attach a GraphicsRaycaster component to your canvas by clicking the Add Component button in the Inspector window.
//Also make sure you have an EventSystem in your hierarchy.

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class CheckForUIHit : MonoBehaviour
{
    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    private ColorJoystickTouchController joystickController;

    void Start()
    {
        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();
        joystickController = GameObject.FindWithTag("ColorJoystickController").GetComponent<ColorJoystickTouchController>();
    }

    // TODO: Move out of Update to touch controller, so the code to detect whether joystick was touchec/clicked only runs once per touch
    void Update()
    {
        // Scale the color joystick up to a usable size, but only while being used
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            joystickController.ScaleBack();
            return;
        }

        //Check if the left Mouse button is clicked
        if ((Input.touchCount > 0) || Input.GetKey(KeyCode.Mouse0))
        {

            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);



            #if UNITY_EDITOR
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;
            #else
            //Set the Pointer Event Position to that of the touch position
            m_PointerEventData.position = Input.touches[0].position;
            #endif

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                Debug.Log("Hit " + result.gameObject.name);
                if (result.gameObject.CompareTag("ColorJoystick")) 
                {
                    // Set a flag to pass to the touch event, so scaling up only happens when joystick is touched
                    joystickController.isJoystickTouched = true;
                    joystickController.ScaleUp();
                }
            }
        }

    }

}