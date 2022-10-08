using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreathAnimation : MonoBehaviour
{
    private const float BREATH_SCALE_MIN = 0.002f;
    private const float BREATH_SCALE_MAX = 0.004f;
    private const float BREATH_SCALE_RANGE = BREATH_SCALE_MAX - BREATH_SCALE_MIN;

    private float _lastBRValue;
    private float _beatStartTime;
    private float _beatCompleteTime;
    private float _rrDuration; // time in seconds for one beat

    public SkeletonVisualization skeletonVisualization;
    public SkeletonManager skeletonManager;
    public TextMesh breathTextmesh;
    private GameObject BreathVizGO;

    private void Start()
    {
        _lastBRValue = 0;
        BreathVizGO = this.transform.parent.gameObject;
    }

    private void Update()
    {
        if (skeletonVisualization != null && skeletonVisualization.Visible == true)
        {
            BreathVizGO.transform.position = skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.SpineMid);
        }

        if (skeletonManager != null) _lastBRValue = skeletonManager.BreathRate;

        // animate breath
        if (_lastBRValue != 0)
        {
            breathTextmesh.text = _lastBRValue.ToString("0");

            float tBeatPhase = 0;
            if (Time.time >= _beatCompleteTime)
            {
                _rrDuration = 60f / _lastBRValue;
                _beatStartTime = Time.time;
                _beatCompleteTime = Time.time + _rrDuration;
                tBeatPhase = 0;
            }
            else
            {
                tBeatPhase = (Time.time - _beatStartTime) / _rrDuration;
            }

            float tBeatPhaseSmoothed = Mathf.SmoothStep(0, 1, tBeatPhase);
            float tScale = BREATH_SCALE_MAX - (BREATH_SCALE_RANGE * tBeatPhaseSmoothed);

            Mathf.Clamp(tScale, BREATH_SCALE_MIN, BREATH_SCALE_MAX);
            this.transform.localScale = new Vector3(tScale, this.transform.localScale.y, tScale);
        }
    }
}
