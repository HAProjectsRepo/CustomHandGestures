using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using TMPro;
using Unity.Mathematics;

// Control Table class to control the gesture recording and detection
public class ControlTable : MonoBehaviour
{
    // Reference to custom gesture object
    [SerializeField] CustomGesture customGesture;
    
    // Reference for hand toggle button text
    [SerializeField] TextMeshPro handText;
    
    // Reference for mode toggle button text
    [SerializeField] TextMeshPro modeText;
    
    // Reference for hand toggle button
    [SerializeField] GameObject handButton;
    
    // Reference for mode toggle button
    [SerializeField] GameObject modeButton;

    // Current hand selection
    HandSelection currentHandSelection = HandSelection.RightHand;
    
    // Current mode
    string currentMode = "None";
    
    // Coroutine for detecting gesture
    Coroutine detectGestureCoroutine;

    // Start is called before the first frame update
    void Start()
    {
        // Subscribe to the gesture recorded event
        customGesture.OnGestureRecorded += ToggleMode;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Toggle hand
    [ContextMenu("Toggle Hand")]
    public void ToggleHand()
    {
        // Rotate between left hand, right hand and both hands
        if(currentHandSelection == HandSelection.LeftHand)
        {
            currentHandSelection = HandSelection.RightHand;
            handText.text = "Right Hand";
        }
        else if(currentHandSelection == HandSelection.RightHand)
        {
            currentHandSelection = HandSelection.BothHands;
            handText.text = "Both Hands";
        }
        else
        {
            currentHandSelection = HandSelection.LeftHand;
            handText.text = "Left Hand";
        }

        // Stop the current detection coroutine and start a new one if hand is changed
        StopCoroutine(detectGestureCoroutine);
        customGesture.StopDetection();
        detectGestureCoroutine = StartCoroutine(DetectGesture());
    }

    // Toggle mode
    [ContextMenu("Toggle Mode")]
    public void ToggleMode()
    {
        // Toggle between record gesture and detect gesture
        if(currentMode == "Detect Gesture" || currentMode == "None")
        {
            currentMode = "Record Gesture";
            handButton.SetActive(false);
            modeButton.SetActive(false);
            customGesture.Record(currentHandSelection);
        }
        else
        {
            currentMode = "Detect Gesture";
            handButton.SetActive(true);
            modeButton.SetActive(true);
            detectGestureCoroutine = StartCoroutine(DetectGesture());
        }
        modeText.text = currentMode;
    }

    // Detect gesture Courotine
    IEnumerator DetectGesture()
    {
        // Wait for 1 second before starting the detection
        yield return new WaitForSeconds(1);
        customGesture.StartDetection(currentHandSelection);
        while(currentMode == "Detect Gesture") yield return null;
        customGesture.StopDetection();
    }
}

// Enum for selected hand type
public enum HandSelection
{
    LeftHand,
    RightHand,
    BothHands
}