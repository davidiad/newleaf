﻿/*
about this script: 

if this single joystick is not set to stay in a fixed position
 this script makes it appear and disappear where the screen was touched (if set to always stay visible this single joystick will just disappear from its current position and appear where the screen was touches and stay visible at the new position)

if this joystick is set to stay in a fixed position
 this script makes it appear and disappear if the user touches within the area of its background image (even if it is not currently visible)
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ColorJoystickTouchController : MonoBehaviour
{
    public bool isJoystickTouched;
    public Image singleJoystickBackgroundImage; // background image of the single joystick (the joystick's handle (knob) is a child of this image and moves along with it)
    public bool singleJoyStickAlwaysVisible = true; // value from single joystick that determines if the single joystick should be always visible or not

    private Image singleJoystickHandleImage; // handle (knob) image of the single joystick
    private GameObject joystick;
    private ColorJoystick colorJoystick; // script component attached to the single joystick's background image
    private int singleSideFingerID = 0; // unique finger id for touches on the left-side half of the screen

    public float scaleFactor = 2.0f; // amount to scale up joystick when touched
    private Vector3 scaleVector;

    private PaintManager paintManager;
    private Vector3 initialPosition;
    private float shift;

    void Start()
    {
        isJoystickTouched = false;
        //if (singleJoystickBackgroundImage.GetComponent<ColorJoystick>() == null)
        //{
        //    Debug.LogError("There is no joystick attached to this script.");
        //}
        //else
        //{
        //    colorJoystick = singleJoystickBackgroundImage.GetComponent<ColorJoystick>(); // gets the single joystick script
        //    singleJoystickBackgroundImage.enabled = singleJoyStickAlwaysVisible; // sets single joystick background image to be always visible or not
        //}
        joystick = GameObject.FindWithTag("ColorJoystick");
        colorJoystick = joystick.GetComponent<ColorJoystick>();
        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
        scaleVector = new Vector3(scaleFactor, scaleFactor, scaleFactor);

        if (colorJoystick == null)
        {
            Debug.LogError("There is no joystick attached to this script.");
        }
        else
        {
            colorJoystick.GetComponent<ColorJoystick>(); // gets the color joystick script
            singleJoystickBackgroundImage.enabled = singleJoyStickAlwaysVisible; // sets single joystick background image to be always visible or not
        }

        if (colorJoystick.transform.GetChild(0).GetComponent<Image>() == null)
        {
            Debug.LogError("There is no joystick handle (knob) attached to this script.");
        }
        else
        {
            singleJoystickHandleImage = colorJoystick.transform.GetChild(0).GetComponent<Image>(); // gets the handle (knob) image of the single joystick
            singleJoystickHandleImage.enabled = singleJoyStickAlwaysVisible; // sets single joystick handle (knob) image to be always visible or not
        }
        initialPosition = joystick.transform.localPosition;
        shift = (200f * scaleFactor) * 0.5f - 100f; // assuming button width = 200
    }

    void FixedUpdate()
    {
        // can move code from FixedUpdate() to Update() if your controlled object does not use physics
        // can move code from Update() to FixedUpdate() if your controlled object does use physics
        // can see which one works best for your project
    }

    void Update()
    {
        // if the screen has been touched
        if (Input.touchCount > 0)
        {
            Touch[] myTouches = Input.touches; // gets all the touches and stores them in an array

            // loops through all the current touches
            for (int i = 0; i < Input.touchCount; i++)
            {
                // if this touch just started (finger is down for the first time), for this particular touch 
                if (myTouches[i].phase == TouchPhase.Began)
                {
                    if (isJoystickTouched)
                    {
                        ScaleUp();
                    }
                    singleSideFingerID = myTouches[i].fingerId; // stores the unique id for this touch that happened on the left-side half of the screen

                        // if the single joystick will drag with any touch (single joystick is not set to stay in a fixed position)
                        if (colorJoystick.joystickStaysInFixedPosition == false)
                        {
                            var currentPosition = singleJoystickBackgroundImage.rectTransform.position; // gets the current position of the single joystick
                            currentPosition.x = myTouches[i].position.x + singleJoystickBackgroundImage.rectTransform.sizeDelta.x / 2; // calculates the x position of the single joystick to where the screen was touched
                            currentPosition.y = myTouches[i].position.y - singleJoystickBackgroundImage.rectTransform.sizeDelta.y / 2; // calculates the y position of the single joystick to where the screen was touched

                            // keeps this single joystick within the screen
                            currentPosition.x = Mathf.Clamp(currentPosition.x, 0 + singleJoystickBackgroundImage.rectTransform.sizeDelta.x, Screen.width);
                            currentPosition.y = Mathf.Clamp(currentPosition.y, 0, Screen.height - singleJoystickBackgroundImage.rectTransform.sizeDelta.y);

                            singleJoystickBackgroundImage.rectTransform.position = currentPosition; // sets the position of the single joystick to where the screen was touched (limited to the left half of the screen)

                            // enables single joystick on touch
                            singleJoystickBackgroundImage.enabled = true;
                            singleJoystickBackgroundImage.rectTransform.GetChild(0).GetComponent<Image>().enabled = true;
                        }
                        else
                        {
                            // single joystick stays fixed, does not set position of single joystick on touch

                            // if the touch happens within the fixed area of the single joystick's background image within the x coordinate
                            if ((myTouches[i].position.x <= singleJoystickBackgroundImage.rectTransform.position.x) && (myTouches[i].position.x >= (singleJoystickBackgroundImage.rectTransform.position.x - singleJoystickBackgroundImage.rectTransform.sizeDelta.x)))
                            {
                                // and the touch also happens within the single joystick's background image y coordinate
                                if ((myTouches[i].position.y >= singleJoystickBackgroundImage.rectTransform.position.y) && (myTouches[i].position.y <= (singleJoystickBackgroundImage.rectTransform.position.y + singleJoystickBackgroundImage.rectTransform.sizeDelta.y)))
                                {
                                    // makes the single joystick appear 
                                    singleJoystickBackgroundImage.enabled = true;
                                    singleJoystickBackgroundImage.rectTransform.GetChild(0).GetComponent<Image>().enabled = true;
                                }
                            }
                        }
                }

                //if (myTouches[i].phase == TouchPhase.Moved || myTouches[i].phase == TouchPhase.Stationary) {
                //    paintManager.UpdateSV();
                //}
                

                // if this touch has ended (finger is up and now off of the screen), for this particular touch 
                if (myTouches[i].phase == TouchPhase.Ended)
                {
                    ScaleBack();
                    // if this touch is the touch that began on the left half of the screen
                    if (myTouches[i].fingerId == singleSideFingerID)
                    {
                        // makes the single joystick disappear or stay visible
                        singleJoystickBackgroundImage.enabled = singleJoyStickAlwaysVisible;
                        singleJoystickHandleImage.enabled = singleJoyStickAlwaysVisible;
                    }
                }
            }
        }
    }

    public void ScaleBack()
    {
        joystick.transform.localScale = Vector3.one;
        joystick.transform.localPosition = initialPosition;
        isJoystickTouched = false;
    }

    public void ScaleUp()
    {
        joystick.transform.localScale = scaleVector;
        joystick.transform.localPosition = new Vector3(initialPosition.x + shift, initialPosition.y - shift, 0f);
    }

}
