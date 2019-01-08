using UnityEngine;
using UnityEngine.EventSystems;
using DigitalRubyShared; // Fingers Gesture Recognizer

public class CanvasGestures : MonoBehaviour
{
    public GameObject colorWheel;
    private PaintManager paintManager;
    private RectTransform colorWheelRectTransform;
    private Vector3 wheelPos;
    private Vector3 previousDir;
    private float rotZ = 0.0f;
    private float angleChange = 0.0f;

    public TapGestureRecognizer tapColorWheelGesture { get; private set; }

    void Start()
    {
        colorWheelRectTransform = colorWheel.GetComponent<RectTransform>();
        paintManager = GameObject.FindWithTag("PaintManager").GetComponent<PaintManager>();
        CreateTapGesture();
    }

    private void TapGestureCallback(GestureRecognizer gesture)
    {
        Debug.Log("Canvas: " + gesture.State.ToString());
        if (gesture.State == GestureRecognizerState.Began)
        {
            
        }
        if (gesture.State == GestureRecognizerState.Ended)
        {
            PresentColorWheel(gesture);
        }
    }

    private void PresentColorWheel(GestureRecognizer gesture)
    {
        if (colorWheel.activeSelf)
        {
            colorWheel.SetActive(false);
        } 
        else
        {
            colorWheel.SetActive(true);
            Debug.Log("Tapped at: " + gesture.FocusX + ", " + gesture.FocusY);
            Vector3 v = colorWheelRectTransform.position;
            Quaternion rot = colorWheelRectTransform.rotation;
            colorWheelRectTransform.SetPositionAndRotation(new Vector3(gesture.FocusX, gesture.FocusY, colorWheelRectTransform.position.z), rot);
        }
    }


    private void CreateTapGesture()
    {
        tapColorWheelGesture = new TapGestureRecognizer();
        tapColorWheelGesture.StateUpdated += TapGestureCallback;
        tapColorWheelGesture.PlatformSpecificView = this.gameObject;
        //tapGesture.RequireGestureRecognizerToFail = doubleTapGesture;
        FingersScript.Instance.AddGesture(tapColorWheelGesture);
    }
}