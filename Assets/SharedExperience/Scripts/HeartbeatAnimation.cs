using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//piece of code from https://assetstore.unity.com/packages/tools/input-management/windows-and-hololens-heart-rate-monitor-plugin-76113
public class HeartbeatAnimation : MonoBehaviour
{
    private const float HEART_SCALE_MIN = 0.03f;
    private const float HEART_SCALE_MAX = 0.04f;
    private const float HEART_SCALE_RANGE = HEART_SCALE_MAX - HEART_SCALE_MIN;

    private float _lastHRValue;
    private float _beatStartTime;
    private float _beatCompleteTime;
    private float _rrDuration; // time in seconds for one heart beat

    public SkeletonVisualization skeletonVisualization;
    public SkeletonManager skeletonManager;
    public TextMesh heartTextmesh;

    private void Start()
    {
        _lastHRValue = 0;
    }
    
    private void Update()
    {
        if (skeletonVisualization != null && skeletonVisualization.Visible == true)
        {
            this.transform.position = (skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.SpineMid) + skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.SpineShoulder)) / 2;
        }

        if (skeletonManager != null) _lastHRValue = skeletonManager.HeartRate;
        
        // animate heart
        if (_lastHRValue != 0)
        {
            heartTextmesh.text = _lastHRValue.ToString("0");

            float tBeatPhase = 0;
            if (Time.time >= _beatCompleteTime)
            {
                _rrDuration = 60f / _lastHRValue;
                _beatStartTime = Time.time;
                _beatCompleteTime = Time.time + _rrDuration;
                tBeatPhase = 0;
            }
            else
            {
                tBeatPhase = (Time.time - _beatStartTime) / _rrDuration;
            }

            float tBeatPhaseSmoothed = Mathf.SmoothStep(0, 1, tBeatPhase);
            float tScale = HEART_SCALE_MAX - (HEART_SCALE_RANGE * tBeatPhaseSmoothed);

            Mathf.Clamp(tScale, HEART_SCALE_MIN, HEART_SCALE_MAX);
            this.transform.localScale = new Vector3(tScale, tScale, tScale);
        }
    }
}
