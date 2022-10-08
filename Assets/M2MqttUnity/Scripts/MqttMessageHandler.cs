using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using Microsoft.MixedReality.Toolkit.UI;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using Vuforia;

public class MqttMessageHandler : MonoBehaviour
{
    public BaseClient baseClient;
    public GameObject ImageTargetGO, PlatformTargetGO;
    public GameObject StonesOriginGO, PlatformOriginGO, PlatformGO;

    private void Start()
    {
        PlatformGO.GetComponent<MeshRenderer>().enabled = true;
    }

    private void OnEnable()
    {
        baseClient.RegisterTopicHandler("M2MQTT/Surgery/CalibrateKinect", HandleCalibration);
        baseClient.RegisterTopicHandler("M2MQTT/Surgery/CalibratePlatform", HandleCalibration);
    }

    private void OnDisable()
    {
        baseClient.UnregisterTopicHandler("M2MQTT/Surgery/CalibrateKinect", HandleCalibration);
        baseClient.UnregisterTopicHandler("M2MQTT/Surgery/CalibratePlatform", HandleCalibration);
    }

    private void HandleCalibration(string topic, string message)
    {
        if (topic == "M2MQTT/Surgery/CalibrateKinect" && message == "true") StartCalibrationKinect();
        if (topic == "M2MQTT/Surgery/CalibrateKinect" && message == "false") StopCalibrationKinect();
        if (topic == "M2MQTT/Surgery/CalibratePlatform" && message == "true") StartCalibrationPlatform();
        if (topic == "M2MQTT/Surgery/CalibratePlatform" && message == "false") StopCalibrationPlatform();
    }

    public void StartCalibrationKinect()
    {
        ImageTargetGO.SetActive(true);
        StartVuforiaCamera();
    }

    public void StopCalibrationKinect()
    {
        StonesOriginGO.transform.position = ImageTargetGO.transform.position;
        StonesOriginGO.transform.rotation = ImageTargetGO.transform.rotation;
        ImageTargetGO.SetActive(false);
        StopVuforiaCamera();
    }

    public void StartCalibrationPlatform()
    {
        PlatformTargetGO.SetActive(true);
        PlatformGO.transform.SetParent(PlatformTargetGO.transform);
        StartVuforiaCamera();
    }

    public void StopCalibrationPlatform()
    {
        PlatformOriginGO.transform.position = PlatformTargetGO.transform.position;
        PlatformOriginGO.transform.rotation = PlatformTargetGO.transform.rotation;
        PlatformGO.transform.SetParent(PlatformOriginGO.transform);
        PlatformTargetGO.SetActive(false);
        StopVuforiaCamera();
    }

    private void StartVuforiaCamera()
    {
        if (!Vuforia.CameraDevice.Instance.IsActive())
        {
            Vuforia.CameraDevice.Instance.Start();
        }
    }

    private void StopVuforiaCamera()
    {
        if (Vuforia.CameraDevice.Instance.IsActive())
        {
            Vuforia.CameraDevice.Instance.Stop();
        }
    }
    
}