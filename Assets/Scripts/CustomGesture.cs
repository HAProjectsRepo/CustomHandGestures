using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.Hands;

// Custom Gesture class to record and detect gestures
public class CustomGesture : MonoBehaviour
{
    // Wait time before gesture is recorded
    [SerializeField] float recordWaitTime = 2.0f;
    
    // Reference to info text
    [SerializeField] TextMeshPro infoText;
    
    // Position threshold for gesture detection
    [SerializeField] float positionThreshold = 0.5f;
    
    // Rotation threshold for gesture detection
    [SerializeField] float rotationThreshold = 250f;
    
    // Delegate for gesture recorded event
    public delegate void GestureRecorded();
    
    // Event for gesture recorded
    public event GestureRecorded OnGestureRecorded;
    
    // Audio source for gesture detection completion sound
    AudioSource completeSound;

    // Start time for gesture
    float startTime = 0;

    // Record and detect flags    
    bool recordLeft = false;    
    bool recordRight = false;    
    bool detectLeft = false;    
    bool detectRight = false;
    bool leftDetected = false;
    bool rightDetected = false;
    bool leftInitialized = false;
    bool rightInitialized = false;
    
    // Recorded and detected poses data (with joints)
    Pose leftRecordedRootPose;
    Pose[] leftRecordedPoses;
    XRHandJoint[] xrLeftRecordedHandJoints;
    Pose rightRecordedRootPose;
    Pose[] rightRecordedPoses;
    XRHandJoint[] xrRightRecordedHandJoints;

    // Start is called before the first frame update
    void Start()
    {
        completeSound = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Record gesture
    public void Record(HandSelection handSelection)
    {
        startTime = Time.time;
        StartCoroutine(RecordCoroutine(handSelection));
    }

    // Record gesture courotine
    IEnumerator RecordCoroutine(HandSelection handSelection)
    {
        // Wait for record wait time and update text
        while(Time.time - startTime < recordWaitTime)
        {
            infoText.text = "Record Gesture in " + (recordWaitTime - (Time.time - startTime)).ToString("0.00") + " secs";
            yield return null;
        }

        // Select the recording hand and set the record flags
        if(handSelection == HandSelection.LeftHand || handSelection == HandSelection.BothHands)
        {
            recordLeft = true;
        }
        if(handSelection == HandSelection.RightHand || handSelection == HandSelection.BothHands)
        {
            recordRight = true;
        }

        // Wait for selected hand/s to be recorded
        while(recordLeft || recordRight)
        {
            yield return null;
        }

        // Play the completion sound, invoke recorded event and update text
        completeSound.Play();
        OnGestureRecorded?.Invoke();
        infoText.text = "Gesture Recorded";
    }

    // On XR Hand Record Update Left
    public void OnXRHandRecordUpdateLeft(XRHandJointsUpdatedEventArgs args)
    {
        // Record the left hand joints if left hand is selected
        if(args.hand.handedness == Handedness.Left && recordLeft)
        {
            // Record the root pose and joint poses for wrist and finger tips
            leftRecordedRootPose = args.hand.rootPose;
            leftRecordedPoses = new Pose[6];
            xrLeftRecordedHandJoints = new XRHandJoint[6];
            xrLeftRecordedHandJoints[0] = args.hand.GetJoint(XRHandJointID.Wrist);
            xrLeftRecordedHandJoints[1] = args.hand.GetJoint(XRHandJointID.ThumbTip);
            xrLeftRecordedHandJoints[2] = args.hand.GetJoint(XRHandJointID.IndexTip);
            xrLeftRecordedHandJoints[3] = args.hand.GetJoint(XRHandJointID.MiddleTip);
            xrLeftRecordedHandJoints[4] = args.hand.GetJoint(XRHandJointID.RingTip);
            xrLeftRecordedHandJoints[5] = args.hand.GetJoint(XRHandJointID.LittleTip);

            // Save the recorded poses position and rotation
            for(int i=0; i<leftRecordedPoses.Length; i++)
            {
                if (xrLeftRecordedHandJoints[i].TryGetPose(out Pose pose))
                {
                    leftRecordedPoses[i].position = leftRecordedRootPose.position - pose.position;
                    leftRecordedPoses[i].rotation = pose.rotation;
                }
            }

            recordLeft = false;
            leftInitialized = true;
        }
    }

    // On XR Hand Record Update Right
    public void OnXRHandRecordUpdateRight(XRHandJointsUpdatedEventArgs args)
    {
        // Record the right hand joints if right hand is selected
        if(args.hand.handedness == Handedness.Right && recordRight)
        {
            // Record the root pose and joint poses for wrist and finger tips
            rightRecordedRootPose = args.hand.rootPose;
            rightRecordedPoses = new Pose[6];
            xrRightRecordedHandJoints = new XRHandJoint[6];
            xrRightRecordedHandJoints[0] = args.hand.GetJoint(XRHandJointID.Wrist);
            xrRightRecordedHandJoints[1] = args.hand.GetJoint(XRHandJointID.ThumbTip);
            xrRightRecordedHandJoints[2] = args.hand.GetJoint(XRHandJointID.IndexTip);
            xrRightRecordedHandJoints[3] = args.hand.GetJoint(XRHandJointID.MiddleTip);
            xrRightRecordedHandJoints[4] = args.hand.GetJoint(XRHandJointID.RingTip);
            xrRightRecordedHandJoints[5] = args.hand.GetJoint(XRHandJointID.LittleTip);

            // Save the recorded poses position and rotation
            for(int i=0; i<rightRecordedPoses.Length; i++)
            {
                if (xrRightRecordedHandJoints[i].TryGetPose(out Pose pose))
                {
                    rightRecordedPoses[i].position = rightRecordedRootPose.position - pose.position;
                    rightRecordedPoses[i].rotation = pose.rotation;
                }
            }

            recordRight = false;
            rightInitialized = true;
        }
    }

    // On XR Hand Detect Update Left
    public void OnXRHandDetectUpdateLeft(XRHandJointsUpdatedEventArgs args)
    {
        // Get the left hand joints if left hand is selected
        if(args.hand.handedness == Handedness.Left && detectLeft && leftInitialized)
        {
            // Get the root pose and joint poses for wrist and finger tips
            Pose leftDetectRootPose = args.hand.rootPose;
            Pose[] leftDetectPoses = new Pose[6];
            XRHandJoint[] xrLeftDetectHandJoints = new XRHandJoint[6];
            xrLeftDetectHandJoints[0] = args.hand.GetJoint(XRHandJointID.Wrist);
            xrLeftDetectHandJoints[1] = args.hand.GetJoint(XRHandJointID.ThumbTip);
            xrLeftDetectHandJoints[2] = args.hand.GetJoint(XRHandJointID.IndexTip);
            xrLeftDetectHandJoints[3] = args.hand.GetJoint(XRHandJointID.MiddleTip);
            xrLeftDetectHandJoints[4] = args.hand.GetJoint(XRHandJointID.RingTip);
            xrLeftDetectHandJoints[5] = args.hand.GetJoint(XRHandJointID.LittleTip);

            // Get the detected poses position and rotation
            for(int i=0; i<leftDetectPoses.Length; i++)
            {
                if (xrLeftDetectHandJoints[i].TryGetPose(out Pose pose))
                {
                    leftDetectPoses[i].position = leftDetectRootPose.position - pose.position;
                    leftDetectPoses[i].rotation = pose.rotation;
                }
            }

            // Check if the recorded poses are available
            if(leftRecordedRootPose == null)
            {
                leftDetected = false;
                return;
            }

            // Compare the detected poses with the recorded poses with position and rotation thresholds
            float positionDifference = 0;
            float rotationDifference = 0;
            for(int j=0; j<leftDetectPoses.Length; j++)
            {
                positionDifference += Vector3.Distance(leftDetectPoses[j].position, leftRecordedPoses[j].position);
                rotationDifference += Quaternion.Angle(leftDetectPoses[j].rotation, leftRecordedPoses[j].rotation);
            }

            // Set the detected flag based on the position and rotation differences
            if(positionDifference <= positionThreshold && rotationDifference <= rotationThreshold)
            {
                leftDetected = true;
            }
            else
            {
                leftDetected = false;
            }
        }
    }

    // On XR Hand Detect Update Right
    public void OnXRHandDetectUpdateRight(XRHandJointsUpdatedEventArgs args)
    {
        // Get the right hand joints if right hand is selected
        if(args.hand.handedness == Handedness.Right && detectRight && rightInitialized)
        {
            // Get the root pose and joint poses for wrist and finger tips
            Pose rightDetectRootPose = args.hand.rootPose;
            Pose[] rightDetectPoses = new Pose[6];
            XRHandJoint[] xrRightDetectHandJoints = new XRHandJoint[6];
            xrRightDetectHandJoints[0] = args.hand.GetJoint(XRHandJointID.Wrist);
            xrRightDetectHandJoints[1] = args.hand.GetJoint(XRHandJointID.ThumbTip);
            xrRightDetectHandJoints[2] = args.hand.GetJoint(XRHandJointID.IndexTip);
            xrRightDetectHandJoints[3] = args.hand.GetJoint(XRHandJointID.MiddleTip);
            xrRightDetectHandJoints[4] = args.hand.GetJoint(XRHandJointID.RingTip);
            xrRightDetectHandJoints[5] = args.hand.GetJoint(XRHandJointID.LittleTip);

            // Get the detected poses position and rotation
            for(int i=0; i<rightDetectPoses.Length; i++)
            {
                if (xrRightDetectHandJoints[i].TryGetPose(out Pose pose))
                {
                    rightDetectPoses[i].position = rightDetectRootPose.position - pose.position;
                    rightDetectPoses[i].rotation = pose.rotation;
                }
            }

            // Check if the recorded poses are available
            if(rightRecordedRootPose == null)
            {
                rightDetected = false;
                return;
            }

            // Compare the detected poses with the recorded poses with position and rotation thresholds
            float positionDifference = 0;
            float rotationDifference = 0;
            for(int j=0; j<rightDetectPoses.Length; j++)
            {
                positionDifference += Vector3.Distance(rightDetectPoses[j].position, rightRecordedPoses[j].position);
                rotationDifference += Quaternion.Angle(rightDetectPoses[j].rotation, rightRecordedPoses[j].rotation);
            }
            
            // Set the detected flag based on the position and rotation differences
            if(positionDifference <= positionThreshold && rotationDifference <= rotationThreshold)
            {
                rightDetected = true;
            }
            else
            {
                rightDetected = false;
            }
        }
    }

    // Start Detection
    public void StartDetection(HandSelection handSelection)
    {
        // Start the detection coroutine and set the detect flags
        if(handSelection == HandSelection.LeftHand || handSelection == HandSelection.BothHands)
        {
            detectLeft = true;
        }
        if(handSelection == HandSelection.RightHand || handSelection == HandSelection.BothHands)
        {
            detectRight = true;
        }
        leftDetected = false;
        rightDetected = false;
        StartCoroutine(DetectCoroutine(handSelection));
    }

    // Detect gesture Courotine
    IEnumerator DetectCoroutine(HandSelection handSelection)
    {
        // Keep on detecting till flags are set
        while(detectLeft || detectRight)
        {
            if(handSelection == HandSelection.LeftHand) infoText.text = leftDetected? "Gesture Detected": infoText.text = "Gesture Not Detected";
            else if(handSelection == HandSelection.RightHand) infoText.text = rightDetected? "Gesture Detected": infoText.text = "Gesture Not Detected";
            else infoText.text = leftDetected && rightDetected? "Gesture Detected": infoText.text = "Gesture Not Detected";
            yield return null;
        }
    }

    // Stop Detection
    public void StopDetection()
    {
        // Stop the detection coroutine and reset the detect flags
        detectLeft = false;
        detectRight = false;
    }

}
