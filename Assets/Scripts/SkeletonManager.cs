using UnityEngine;

using M2MqttUnity;

using SimpleJSON;

using System.Collections.Generic;

public class SkeletonManager : MonoBehaviour
{
    public BaseClient baseClient;
    public ISkeletonProvider skeletonProvider;
    public SkeletonVisualization skeletonVisualization;
    
    private float _heartRate, _breathRate;
    public float HeartRate
    {
        get { return _heartRate; }
    }
    public float BreathRate
    {
        get { return _breathRate; }
    }

    private void Start()
    {
        skeletonVisualization.Visible = false;
    }

    private void Update()
    {
        UpdateAvatarVisualization();
    }

    private void OnEnable()
    {
        baseClient.RegisterTopicHandler("M2MQTT/ShowSkeletonVisualization", HandleSkeletonVisualization);
        baseClient.RegisterTopicHandler("M2MQTT/HeartbeatData", HandleHeartbeatMessage);
        baseClient.RegisterTopicHandler("M2MQTT/BreathrateData", HandleBreathRateMessage);
    }

    private void OnDisable()
    {
        baseClient.UnregisterTopicHandler("M2MQTT/ShowSkeletonVisualization", HandleSkeletonVisualization);
        baseClient.UnregisterTopicHandler("M2MQTT/HeartbeatData", HandleHeartbeatMessage);
        baseClient.UnregisterTopicHandler("M2MQTT/BreathrateData", HandleBreathRateMessage);
    }

    private void UpdateAvatarVisualization()
    {
        if (skeletonVisualization == null || skeletonProvider == null)
        {
            return;
        }

        Dictionary<Windows.Kinect.JointType, Vector3> jointPositions = skeletonProvider.GetJointPositions();
        skeletonVisualization.SetJointPositions(jointPositions);

        UpdateJointAngle();
    }

    private void HandleSkeletonVisualization(string topic, string message)
    {
        if (topic == "M2MQTT/ShowSkeletonVisualization" && message == "true") skeletonVisualization.Visible = true;
        if (topic == "M2MQTT/ShowSkeletonVisualization" && message == "false") skeletonVisualization.Visible = false;
    }

    private void HandleHeartbeatMessage(string topic, string message)
    {
        _heartRate = float.Parse(message);
    }

    private void HandleBreathRateMessage(string topic, string message)
    {
        _breathRate = float.Parse(message);
    }


    private void UpdateJointAngle()
    {
        Vector3 leftBone1 = skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ElbowLeft).normalized - skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ShoulderLeft).normalized;
        Vector3 leftBone2 = skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ElbowLeft).normalized - skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.WristLeft).normalized;
        Vector3 rightBone1 = skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ElbowRight).normalized - skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ShoulderRight).normalized;
        Vector3 rightBone2 = skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.ElbowRight).normalized - skeletonVisualization.GetJointWorldPosition(Windows.Kinect.JointType.WristRight).normalized;

        float angleLeftArm = Vector3.Angle(leftBone1, leftBone2);
        float angleRightArm = Vector3.Angle(rightBone1, rightBone2);

        skeletonVisualization.SetBoneColorFromAngle(Windows.Kinect.JointType.ElbowLeft, angleLeftArm);
        skeletonVisualization.SetBoneColorFromAngle(Windows.Kinect.JointType.ElbowRight, angleRightArm);
        
    }
}
