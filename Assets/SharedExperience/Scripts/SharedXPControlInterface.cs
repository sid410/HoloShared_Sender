using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using M2MqttUnity;
using SimpleJSON;
using Microsoft.MixedReality.Toolkit.UI;
using uPLibrary.Networking.M2Mqtt.Messages;
using System.Text;
using Vuforia;
using extOSC;
using System.Linq;
using System;

public class SharedXPControlInterface : MonoBehaviour
{
    //constants for determining visualization color
    private const float maxTableDist = 1266.0f;
    private const float maxObjectAngle = 90.0f;

    //for gradienting the normalized color
    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;


    ////constants for converting pixel to mm
    //private const float xA = 0.7452f;
    //private const float xB = -80.4449f;
    //private const float yA = 0.7511f;
    //private const float yB = -14.3614f;

    //constants for converting pixel to mm (IN EXPERIMENT ROOM)
    private const float p00_Fx = -55.89f;
    private const float p10_Fx = 0.7283f;
    private const float p01_Fx = -0.01048f;
    private const float p20_Fx = 9.616e-06f;
    private const float p11_Fx = 3.686e-06f;
    private const float p02_Fx = 1.093e-06f;

    private const float p00_Fy = 2.549f;
    private const float p10_Fy = 0.001928f;
    private const float p01_Fy = 0.7354f;
    private const float p20_Fy = -9.061e-07f;
    private const float p11_Fy = 9.17e-06f;
    private const float p02_Fy = 3.99e-06f;

    //constants to reproject the correction of objects with height
    private const float camHeight = 0.815f; //-------------- (IN EXPERIMENT ROOM)
    //private const float camHeight = 0.8f;
    private const float bottleHeight = 0.305f;
    private const float glassHeight = 0.16f;
    private const float cupHeight = 0.105f;
    private const float dishHeight = 0.08f;

    //camera plane in xy space coordinate, projected to table
    private const float camCenterPixel_x = 960.0f;
    private const float camCenterPixel_y = 540.0f;
    private float camCenterTable_x, camCenterTable_y;



    private OSCTransmitter Transmitter;
    private AnchorableObject originAnchorTable, originAnchorSkeleton, originAnchorPlatform;
    private ObjectSpawner objectSpawner;
    
    //for evaluation summary board
    private GameObject EvaluationSummary;
    private TextMesh distanceMedianText, angleMedianText, totalTimeText;
    private float distanceMedian, angleMedian;

    public BaseClient baseClient;
    public GameObject ImageTargetGO;
    public GameObject TableOriginGO, SkeletonOriginGO, PlatformOriginGO;

    private bool isRecording = false;
    // ------------state control------------
    public enum AppState
    {
        Setup, Evaluate
    }
    public AppState State
    {
        get;
        set;
    }
    private void ChangeState(AppState newState)
    {
        if (State != newState)
        {
            State = newState;
        }
    }
    // ------------state control------------


    private void Start()
    {
        //disable vuforia once initialized
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(StopVuforiaCamera);

        //initialize transmitter, anchor, and gameobjects
        Transmitter = GameObject.FindObjectOfType<OSCTransmitter>();
        originAnchorTable = TableOriginGO.GetComponent<AnchorableObject>();
        originAnchorSkeleton = SkeletonOriginGO.GetComponent<AnchorableObject>();
        originAnchorPlatform = PlatformOriginGO.GetComponent<AnchorableObject>();
        objectSpawner = GameObject.FindObjectOfType<ObjectSpawner>();

        //for the evaluation summary board
        EvaluationSummary = TableOriginGO.transform.Find("StonesOrigin/EvaluationSummary").gameObject;
        distanceMedianText = EvaluationSummary.transform.Find("DistanceMedian").GetComponent<TextMesh>();
        angleMedianText = EvaluationSummary.transform.Find("AngleMedian").GetComponent<TextMesh>();
        totalTimeText = EvaluationSummary.transform.Find("TotalTime").GetComponent<TextMesh>();

        //calculate projected camera center on table
        camCenterTable_x = PixelToTableX(camCenterPixel_x, camCenterPixel_y);
        camCenterTable_y = PixelToTableY(camCenterPixel_x, camCenterPixel_y);

        SetupMode();

        //set green to red gradient with full red at 1/5 diagonal of table
        SetGradiencyOfMedian(0.2f);
    }

    //IEnumerator LateStart(float waitTime)
    //{
    //    yield return new WaitForSeconds(waitTime);
    //    //Delay the disabling of real
    //    SetupMode();
    //}

    private void OnEnable()
    {
        baseClient.RegisterTopicHandler("M2MQTT/CalibrateSharedSpace/SetReceiverAddress", HandleReceiverAddress);
        baseClient.RegisterTopicHandler("M2MQTT/CalibrateSharedSpace/Sender", HandleCalibration);
        baseClient.RegisterTopicHandler("M2MQTT/Matlab/DataResults", HandleMatlabResults);
        baseClient.RegisterTopicHandler("M2MQTT/StateLogic", HandleStateLogic);
        baseClient.RegisterTopicHandler("M2MQTT/SpawnObject", HandleSpawnObject);
        baseClient.RegisterTopicHandler("M2MQTT/ClearObject", HandleClearObject);
        baseClient.RegisterTopicHandler("M2MQTT/DefaultSetup", HandleDefaultSetup);
        baseClient.RegisterTopicHandler("M2MQTT/SaveObjectPositions", HandleSaveObjectPositions);
        baseClient.RegisterTopicHandler("M2MQTT/LoadObjectPositions", HandleLoadObjectPositions);
        baseClient.RegisterTopicHandler("M2MQTT/TotalTime", HandleTotalTime);
        baseClient.RegisterTopicHandler("M2MQTT/NormalizedColorThreshold", HandleNormalizedColorThreshold);
        baseClient.RegisterTopicHandler("M2MQTT/SendObjectsList", HandleSendObjectsList);
        baseClient.RegisterTopicHandler("M2MQTT/RecordingState", HandleRecordingState);
    }

    private void OnDisable()
    {
        baseClient.UnregisterTopicHandler("M2MQTT/CalibrateSharedSpace/SetReceiverAddress", HandleReceiverAddress);
        baseClient.UnregisterTopicHandler("M2MQTT/CalibrateSharedSpace/Sender", HandleCalibration);
        baseClient.UnregisterTopicHandler("M2MQTT/Matlab/DataResults", HandleMatlabResults);
        baseClient.UnregisterTopicHandler("M2MQTT/StateLogic", HandleStateLogic);
        baseClient.UnregisterTopicHandler("M2MQTT/SpawnObject", HandleSpawnObject);
        baseClient.UnregisterTopicHandler("M2MQTT/ClearObject", HandleClearObject);
        baseClient.UnregisterTopicHandler("M2MQTT/DefaultSetup", HandleDefaultSetup);
        baseClient.UnregisterTopicHandler("M2MQTT/SaveObjectPositions", HandleSaveObjectPositions);
        baseClient.UnregisterTopicHandler("M2MQTT/LoadObjectPositions", HandleLoadObjectPositions);
        baseClient.UnregisterTopicHandler("M2MQTT/TotalTime", HandleTotalTime);
        baseClient.UnregisterTopicHandler("M2MQTT/NormalizedColorThreshold", HandleNormalizedColorThreshold);
        baseClient.UnregisterTopicHandler("M2MQTT/SendObjectsList", HandleSendObjectsList);
        baseClient.UnregisterTopicHandler("M2MQTT/RecordingState", HandleRecordingState);
    }


    // ------------setting other hololens ip address and objects list functions------------
    private void HandleReceiverAddress(string topic, string message)
    {
        SetRemoteHost(message);
    }

    private void SetRemoteHost(string rcvrAddress)
    {
        Transmitter.RemoteHost = rcvrAddress;
        Transmitter.RemotePort = 7000;
    }

    private void HandleSendObjectsList(string topic, string message)
    {
        var objListMsg = new OSCMessage("/spawnObject");

        objListMsg.AddValue(OSCValue.Int(objectSpawner.GetCollidingGameObjectsList().Count));

        foreach (GameObject gObject in objectSpawner.GetCollidingGameObjectsList())
        {
            objListMsg.AddValue(OSCValue.String(gObject.tag));
            objListMsg.AddValue(OSCValue.String(gObject.name));
        }

        Transmitter.Send(objListMsg);
        StartCoroutine(InitializeTransformDelayed(0.2f));
    }
    IEnumerator InitializeTransformDelayed(float waitTime)
    {
        foreach (GameObject gObject in objectSpawner.GetCollidingGameObjectsList())
        {
            yield return new WaitForSeconds(waitTime);
            gObject.GetComponent<TransformSender>().SendUpdatedTransform();
        }
    }
    // ------------setting other hololens ip address and objects list functions------------




    // ------------vuforia calibration functions------------
    private void HandleCalibration(string topic, string message)
    {
        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Table" && message == "true") StartCalibration();
        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Table" && message == "false") StopCalibrationTable();

        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Skeleton" && message == "true") StartCalibration();
        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Skeleton" && message == "false") StopCalibrationSkeleton();

        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Platform" && message == "true") StartCalibration();
        if (topic == "M2MQTT/CalibrateSharedSpace/Sender/Platform" && message == "false") StopCalibrationPlatform();
    }

    public void StartCalibration()
    {
        ImageTargetGO.SetActive(true);
        StartVuforiaCamera();
    }

    public void StopCalibrationTable()
    {
        TableOriginGO.transform.position = ImageTargetGO.transform.position;
        TableOriginGO.transform.rotation = ImageTargetGO.transform.rotation;

        originAnchorTable.SaveAnchor();

        ImageTargetGO.SetActive(false);
        StopVuforiaCamera();
    }

    public void StopCalibrationSkeleton()
    {
        SkeletonOriginGO.transform.position = ImageTargetGO.transform.position;
        SkeletonOriginGO.transform.rotation = ImageTargetGO.transform.rotation;

        originAnchorSkeleton.SaveAnchor();

        ImageTargetGO.SetActive(false);
        StopVuforiaCamera();
    }

    public void StopCalibrationPlatform()
    {
        PlatformOriginGO.transform.position = ImageTargetGO.transform.position;
        PlatformOriginGO.transform.rotation = ImageTargetGO.transform.rotation;

        originAnchorPlatform.SaveAnchor();

        ImageTargetGO.SetActive(false);
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
    // ------------vuforia calibration functions------------
    
    
    
    // ------------State control functions------------
    private void HandleStateLogic(string topic, string message)
    {
        if (topic == "M2MQTT/StateLogic" && message == "Setup") SetupMode();
        if (topic == "M2MQTT/StateLogic" && message == "Evaluate") EvaluationMode();
    }

    private void SetupMode()
    {
        ChangeState(AppState.Setup);
        ShowAllEvaluationVisualizations(false);
    }

    private void EvaluationMode()
    {
        ChangeState(AppState.Evaluate);
        ShowAllEvaluationVisualizations(true);
    }

    private void HandleRecordingState(string topic, string message)
    {
        if (topic == "M2MQTT/RecordingState" && message == "true") isRecording = true;
        if (topic == "M2MQTT/RecordingState" && message == "false") isRecording = false;
    }
    // ------------State control functions------------


    // ------------Visualization functions------------
    private void ShowAllEvaluationVisualizations(bool show)
    {
        foreach (GameObject gObject in objectSpawner.GetCollidingGameObjectsList())
        {
            gObject.GetComponent<DifferenceCalculator>().ShowEvaluationVisualization(show);
        }
        ShowSummaryVisualization(show);
    }

    private void ShowSummaryVisualization(bool isVisible)
    {
        float alpha;
        if (isVisible) alpha = 1.0f;
        else alpha = 0.0f;

        Renderer[] children = EvaluationSummary.GetComponentsInChildren<Renderer>();
        Color newColor;
        foreach (Renderer child in children)
        {
            newColor = child.material.color;
            newColor.a = alpha;
            child.material.color = newColor;
        }
    }

    // ------------Visualization functions------------


    // ------------Utensil Object functions------------
    private void HandleSpawnObject(string topic, string message)
    {
        if (topic == "M2MQTT/SpawnObject") objectSpawner.SpawnUtensil(int.Parse(message));
    }
    private void HandleClearObject(string topic, string message)
    {
        if (topic == "M2MQTT/ClearObject") objectSpawner.ClearAllUtensil();
    }
    private void HandleDefaultSetup(string topic, string message)
    {
        if (topic == "M2MQTT/DefaultSetup") objectSpawner.SpawnAllUtensil();
    }
    private void HandleSaveObjectPositions(string topic, string message)
    {
        if (topic == "M2MQTT/SaveObjectPositions") objectSpawner.SaveObjectTransforms(message);
    }
    private void HandleLoadObjectPositions(string topic, string message)
    {
        if (topic == "M2MQTT/LoadObjectPositions") objectSpawner.LoadSavedObjectTransforms(message);
    }
    // ------------Utensil Object functions------------

    
    // ------------Matlab interpreter------------
    private void HandleMatlabResults(string topic, string message)
    {
        List<GameObject> virtualObjectsList = new List<GameObject>(objectSpawner.GetCollidingGameObjectsList());
        string[] results = message.Split('\n');
        
        foreach (string res in results)
        {
            if (res == "") continue;
            
            string[] data = res.Split(';');
            PairRealToVirtualObjects(data, virtualObjectsList);
        }

        RefreshEvaluationData();
    }

    // ------------Nodered utils------------
    private void HandleTotalTime(string topic, string message)
    {
        if (topic == "M2MQTT/TotalTime")
        {
            string[] minSec = message.Split(':');
            if(minSec[0] == "0") totalTimeText.text = "Total Time: " + minSec[1] + " seconds";
            else totalTimeText.text = "Total Time: " + minSec[0] + " minute " + minSec[1] + " seconds";
        }
    }

    private void HandleNormalizedColorThreshold(string topic, string message)
    {
        if (topic == "M2MQTT/NormalizedColorThreshold")
        {
            float colorThreshold = float.Parse(message);

            foreach (GameObject gObject in objectSpawner.GetCollidingGameObjectsList())
            {
                gObject.GetComponent<DifferenceCalculator>().SetGradiencyOfThisUtensil(colorThreshold);
            }
            SetGradiencyOfMedian(colorThreshold);
        }
    }
    // ------------Nodered utils------------



    // ------------functions for pairing virutal objects set thru manipulation and real objects tracked from camera------------
    private void PairRealToVirtualObjects(string[] uData, List<GameObject> vList)
    {
        float origin_Xmm = PixelToTableX(float.Parse(uData[0]), float.Parse(uData[1]));
        float origin_Ymm = PixelToTableY(float.Parse(uData[0]), float.Parse(uData[1]));
        float axis_Degrees = float.Parse(uData[2]) + 90.0f; //because pixel to table is offset by 90 deg rot in Y
        GameObject virtualGO;
        Vector3 realPos;

        switch (uData[3].Trim())
        {
            case "1":
                realPos = new Vector3(origin_Xmm, 0, origin_Ymm);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Spoon");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, true);
                vList.Remove(virtualGO);
                break;

            case "2":
                realPos = new Vector3(origin_Xmm, 0, origin_Ymm);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Fork");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, true);
                vList.Remove(virtualGO);
                break;

            case "3":
                realPos = CorrectProjectionObjectsWithHeight(origin_Xmm, origin_Ymm, cupHeight);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Cup");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, false);
                vList.Remove(virtualGO);
                break;

            case "4":
                realPos = CorrectProjectionObjectsWithHeight(origin_Xmm, origin_Ymm, dishHeight);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Dish");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, false);
                vList.Remove(virtualGO);
                break;

            case "5":
                realPos = new Vector3(origin_Xmm, 0, origin_Ymm);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Knife");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, true);
                vList.Remove(virtualGO);
                break;

            case "6":
                realPos = new Vector3(origin_Xmm, 0, origin_Ymm);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Minispoon");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, true);
                vList.Remove(virtualGO);
                break;

            case "7":
                realPos = CorrectProjectionObjectsWithHeight(origin_Xmm, origin_Ymm, bottleHeight);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Bottle");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, false);
                vList.Remove(virtualGO);
                break;

            case "8":
                realPos = CorrectProjectionObjectsWithHeight(origin_Xmm, origin_Ymm, glassHeight);
                virtualGO = FindLeastDistanceSameObject(realPos, vList, "Glass");
                SetRealOriginAndAxis(virtualGO, realPos, axis_Degrees, false);
                vList.Remove(virtualGO);
                break;

            case "0":
                //do nothing to unrecognized objects
                break;

            default:
                break;
        }
    }

    private GameObject FindLeastDistanceSameObject(Vector3 realOrigin, List<GameObject> virtualObjects, string label)
    {
        Dictionary<GameObject, float> sameTagObjects = new Dictionary<GameObject, float>();

        foreach (GameObject gObject in virtualObjects)
        {
            if (!gObject.CompareTag(label)) continue;

            float distance = Vector3.Distance(gObject.transform.position, realOrigin);
            sameTagObjects.Add(gObject, distance);
        }

        if (sameTagObjects.Count == 0) return null;

        GameObject nearest = sameTagObjects.FirstOrDefault(x => x.Value == sameTagObjects.Values.Min()).Key;
        return nearest;
    }

    private void SetRealOriginAndAxis(GameObject virtualObject, Vector3 tablePos, float tableRot, bool withAngle)
    {
        if (virtualObject == null) return;

        string realName = virtualObject.name + "_Axis";
        GameObject realGO = GameObject.Find(realName).gameObject;
        realGO.transform.localPosition = tablePos;

        if (withAngle)
        {
            realGO.transform.localEulerAngles = new Vector3(0, tableRot, 0);
            //GameObject realAxis = realGO.transform.Find("Xaxis").gameObject;
            //realAxis.transform.localEulerAngles = new Vector3(0, tableRot, 0);
        }
    }

    private void RefreshEvaluationData()
    {
        List<float> distanceError = new List<float>();
        List<float> angleError = new List<float>();

        string errorMsg = "";
        //string errorMsg = TableToPixelX(0).ToString() + "," + TableToPixelY(0).ToString() + ";";

        foreach (GameObject gObject in objectSpawner.GetCollidingGameObjectsList())
        {
            gObject.GetComponent<DifferenceCalculator>().UpdateEvaluationData();
            errorMsg = errorMsg + gObject.name + ",";

            distanceError.Add(gObject.GetComponent<DifferenceCalculator>().DistanceError);
            errorMsg = errorMsg + gObject.GetComponent<DifferenceCalculator>().DistanceError.ToString() + ",";

            if (gObject.GetComponent<DifferenceCalculator>().WithAngle)
            {
                angleError.Add(gObject.GetComponent<DifferenceCalculator>().AngleError);
                errorMsg = errorMsg + gObject.GetComponent<DifferenceCalculator>().AngleError.ToString() + ",";
            }
            else errorMsg = errorMsg + "0,";

            errorMsg = errorMsg + gObject.GetComponent<DifferenceCalculator>().virtualX + "," + gObject.GetComponent<DifferenceCalculator>().virtualY + "," + gObject.GetComponent<DifferenceCalculator>().realX + "," + gObject.GetComponent<DifferenceCalculator>().realY + ";";
        }
        baseClient.SendErrorEvaluations(errorMsg);

        //get median of error
        distanceMedian = GetMedian(distanceError.ToArray());
        angleMedian = GetMedian(angleError.ToArray());

        //change color based on normalized value
        float normalizedDistanceError = distanceMedian / maxTableDist;
        float normalizedAngleError = angleMedian / maxObjectAngle;
        
        distanceMedianText.text = "Distance: " + distanceMedian.ToString("N1") + "mm";
        distanceMedianText.color = gradient.Evaluate(normalizedDistanceError);

        angleMedianText.text = "Angle: " + angleMedian.ToString("N1") + "°";
        angleMedianText.color = gradient.Evaluate(normalizedAngleError);
    }

    private void SetGradiencyOfMedian(float redColorKey)
    {
        if (!(redColorKey > 0 && redColorKey < 1)) return;

        gradient = new Gradient();

        // set from what key it starts to red
        colorKey = new GradientColorKey[4];
        colorKey[0].color = Color.green;
        colorKey[0].time = 0.0f;
        colorKey[1].color = Color.yellow;
        colorKey[1].time = redColorKey / 2.0f;
        colorKey[2].color = Color.red;
        colorKey[2].time = redColorKey;
        colorKey[3].color = Color.red;
        colorKey[3].time = 1.0f;

        // always opaque
        alphaKey = new GradientAlphaKey[2];
        alphaKey[0].alpha = 1.0f;
        alphaKey[0].time = 0.0f;
        alphaKey[1].alpha = 1.0f;
        alphaKey[1].time = 1.0f;

        gradient.SetKeys(colorKey, alphaKey);
    }
    // ------------functions for pairing virutal objects set thru manipulation and real objects tracked from camera------------


    //CHANGE THIS TO 38/2 AND 26.5/2 the 215 and 145 because of remove white space in stone image
    // ------------functions for converting/correcting pixel to table coordinates------------

    private float PixelToTableX(float x_pixel, float y_pixel)
    {
        float x_table = p00_Fx + (p10_Fx * x_pixel) + (p01_Fx * y_pixel) + (p20_Fx * x_pixel * x_pixel) + (p11_Fx * x_pixel * y_pixel) + (p02_Fx * y_pixel * y_pixel);
        x_table = (x_table - 190) / 1000;
        return x_table;
    }
    private float PixelToTableY(float x_pixel, float y_pixel)
    {
        float y_table = p00_Fy + (p10_Fy * x_pixel) + (p01_Fy * y_pixel) + (p20_Fy * x_pixel * x_pixel) + (p11_Fy * x_pixel * y_pixel) + (p02_Fy * y_pixel * y_pixel);
        y_table = (y_table - 132.5f) / -1000;
        return y_table;
    }

    //old functions that are changed to ones above
    //private float PixelToTableX(float x_pixel)
    //{
    //    float x_table = ((x_pixel * xA) + xB - 190) / 1000;
    //    return x_table;
    //}
    //private float PixelToTableY(float y_pixel)
    //{
    //    float y_table = ((y_pixel * yA) + yB - 132.5f) / -1000;
    //    return y_table;
    //}

    private Vector3 CorrectProjectionObjectsWithHeight(float x_uncorrected, float y_uncorrected, float h_object)
    {
        float correctionRatio = 1 - (h_object / camHeight);
        float x_corrected = ((x_uncorrected - camCenterTable_x) * correctionRatio) + camCenterTable_x;
        float y_corrected = ((y_uncorrected - camCenterTable_y) * correctionRatio) + camCenterTable_y;
        
        return new Vector3(x_corrected, 0, y_corrected);
    }



    ////inverse functions
    //private float TableToPixelX(float x_table)
    //{
    //    float x_pixel = ((x_table * 1000) - xB + 215) / xA;
    //    return x_pixel;
    //}
    //private float TableToPixelY(float y_table)
    //{
    //    float y_pixel = ((y_table * -1000) - yB + 145) / yA;
    //    return y_pixel;
    //}
    // ------------functions for converting/correcting pixel to table coordinates------------


    private static float GetMedian(float[] sourceNumbers)
    {
        if (sourceNumbers == null || sourceNumbers.Length == 0) return 0;

        //make sure the list is sorted, but use a new array
        float[] sortedPNumbers = (float[])sourceNumbers.Clone();
        Array.Sort(sortedPNumbers);

        //get the median
        int size = sortedPNumbers.Length;
        int mid = size / 2;
        float median = (size % 2 != 0) ? (float)sortedPNumbers[mid] : ((float)sortedPNumbers[mid] + (float)sortedPNumbers[mid - 1]) / 2;
        return median;
    }


}
