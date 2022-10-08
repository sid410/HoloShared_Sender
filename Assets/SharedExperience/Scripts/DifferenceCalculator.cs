using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DifferenceCalculator : MonoBehaviour
{
    //constants for determining visualization color
    private const float maxTableDist = 1266.0f;
    private const float maxObjectAngle = 90.0f;

    //for gradienting the normalized color
    private Gradient gradient;
    private GradientColorKey[] colorKey;
    private GradientAlphaKey[] alphaKey;


    //all the real and virtual visualizations
    private GameObject virtualGO, realGO, tableGO;
    private GameObject virtualOrigin, realOrigin, midPointAngle, virtualForward, realForward;
    private LineRenderer distanceLR, angleLR;
    private TextMesh distanceText, angleText;
    private float vertexCount = 12;
    private float midYoffset = 0.1f;

    public Material lineMaterial;
    private Color myColor;
    
    private SharedXPControlInterface m_sharedXPControlInterface;
    private Vector3 lockedPos;
    private Quaternion lockedRot;

    public GameObject AxisAngle, AxisNoAngle;
    private GameObject StonesOrigin;


    //to access the distance and angle error of each utensil
    private float _distanceError, _angleError, _virtualX, _virtualY, _realX, _realY;
    
    public float DistanceError { get { return _distanceError; } }
    public float AngleError { get { return _angleError; } }
    public float virtualX { get { return _virtualX; } }
    public float virtualY { get { return _virtualY; } }
    public float realX { get { return _realX; } }
    public float realY { get { return _realY; } }

    [SerializeField]
    [Tooltip("set to true for spoon, fork, knife, etc...")]
    private bool isWithAngle;

    public bool WithAngle
    {
        get { return isWithAngle; }
    }


    private void Start()
    {
        m_sharedXPControlInterface = GameObject.FindObjectOfType<SharedXPControlInterface>();

        StonesOrigin = GameObject.Find("StonesOrigin");
        tableGO = GameObject.Find("table");
        virtualGO = gameObject;

        //spawn axis prefab
        if (isWithAngle)
        {
            realGO = Instantiate(AxisAngle) as GameObject;
            realGO.transform.parent = StonesOrigin.transform;
            realGO.name = virtualGO.name + "_Axis";
        }
        else
        {
            realGO = Instantiate(AxisNoAngle) as GameObject;
            realGO.transform.parent = StonesOrigin.transform;
            realGO.name = virtualGO.name + "_Axis";
        }
        

        //distance related initializations
        virtualOrigin = virtualGO.transform.Find("origin").gameObject;
        realOrigin = realGO.transform.Find("origin").gameObject;

        distanceText = realOrigin.transform.Find("distanceText").gameObject.GetComponent<TextMesh>();

        distanceLR = new GameObject("LineRenderer").AddComponent<LineRenderer>();
        distanceLR.transform.SetParent(realGO.transform, true);
        distanceLR.startWidth = 0.005f;
        distanceLR.endWidth = 0.005f;
        distanceLR.material = lineMaterial;
        distanceLR.material.color = Color.white;


        //angle related initializations
        if (isWithAngle)
        {
            virtualForward = virtualGO.transform.Find("Xaxis").transform.Find("forwardDir").gameObject;
            realForward = realGO.transform.Find("Xaxis").transform.Find("forwardDir").gameObject;

            midPointAngle = realGO.transform.Find("midPointAngle").gameObject;
            midPointAngle.transform.position = Vector3.Lerp(virtualOrigin.transform.position, realOrigin.transform.position, 0.5f);

            angleText = midPointAngle.transform.Find("angleText").gameObject.GetComponent<TextMesh>();

            angleLR = new GameObject("LineRenderer").AddComponent<LineRenderer>();
            angleLR.transform.SetParent(realGO.transform, true);
            angleLR.startWidth = 0.005f;
            angleLR.endWidth = 0.005f;
            angleLR.material = lineMaterial;
            angleLR.material.color = Color.white;
        }

        //hide at start
        ShowEvaluationVisualization(false);

        //initialize with red
        myColor = this.GetComponent<Renderer>().material.GetColor("_Color");
        this.GetComponent<Renderer>().material.SetColor("_Color", Color.red);

        //set green to red gradient with full red at 1/5 diagonal of table
        SetGradiencyOfThisUtensil(0.2f);
    }


    private void Update()
    {
        //if fall from table or go far away from table, destroy object
        float gap = Vector3.Distance(virtualGO.transform.position, tableGO.transform.position);
        if (gap > 5.0f)
        {
            DestroyObjectInstance();
        }

        //lock the position and rotation during evaluation mode
        if (m_sharedXPControlInterface.State == SharedXPControlInterface.AppState.Setup)
        {
            lockedPos = this.transform.position;
            lockedRot = this.transform.rotation;
        }
        if (m_sharedXPControlInterface.State == SharedXPControlInterface.AppState.Evaluate)
        {
            this.transform.position = lockedPos;
            this.transform.rotation = lockedRot;
        }
    }


    private void CalculateRealToVirtualDistance()
    {
        distanceLR.SetPosition(0, virtualOrigin.transform.position);
        distanceLR.SetPosition(1, realOrigin.transform.position);

        float dist = Vector3.Distance(virtualOrigin.transform.position, realOrigin.transform.position) * 1000;
        distanceText.text = dist.ToString("N1") + "mm";
        
        _distanceError = dist;

        //_virtualX = (virtualOrigin.transform.position.x - StonesOrigin.transform.position.x) * 1000;
        //_virtualY = (virtualOrigin.transform.position.z - StonesOrigin.transform.position.z) * 1000;
        //_realX = (realOrigin.transform.position.x - StonesOrigin.transform.position.x) * 1000;
        //_realY = (realOrigin.transform.position.z - StonesOrigin.transform.position.z) * 1000;

        _virtualX = virtualOrigin.transform.localPosition.x * 1000;
        _virtualY = virtualOrigin.transform.localPosition.z * 1000;
        _realX = realOrigin.transform.localPosition.x * 1000;
        _realY = realOrigin.transform.localPosition.z * 1000;
    }

    private void CalculateRealToVirtualAngle()
    {
        midPointAngle.transform.position = Vector3.Lerp(virtualOrigin.transform.position, realOrigin.transform.position, 0.5f) + new Vector3(0, midYoffset, 0);
        var pointList = new List<Vector3>();

        for (float ratio = 0; ratio <= 1; ratio += 1 / vertexCount)
        {
            var tangent1 = Vector3.Lerp(virtualOrigin.transform.position, midPointAngle.transform.position, ratio);
            var tangent2 = Vector3.Lerp(midPointAngle.transform.position, realOrigin.transform.position, ratio);
            var curve = Vector3.Lerp(tangent1, tangent2, ratio);

            pointList.Add(curve);
        }

        angleLR.positionCount = pointList.Count;
        angleLR.SetPositions(pointList.ToArray());
        
        Vector3 virtualDir = virtualForward.transform.position - virtualOrigin.transform.position;
        Vector3 realDir = realForward.transform.position - realOrigin.transform.position;
        float angle = Vector3.Angle(virtualDir, realDir);
        if (angle > 90.0f) angle = 180.0f - angle;
        angleText.text = angle.ToString("N1") + "°";

        _angleError = angle;
    }

    //make the game object red if not touching the table
    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.name == "table") this.GetComponent<Renderer>().material.SetColor("_Color", myColor);
    }
    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.name == "table") this.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
    }



    //---------public functions----------

    public void UpdateEvaluationData()
    {
        CalculateRealToVirtualDistance();
        if (isWithAngle) CalculateRealToVirtualAngle();

        UpdateColorRepresentation();
    }

    public void UpdateColorRepresentation()
    {
        float normalizedDistanceError = _distanceError / maxTableDist;
        float normalizedAngleError = _angleError / maxObjectAngle;

        distanceLR.material.color = gradient.Evaluate(normalizedDistanceError);
        distanceText.color = gradient.Evaluate(normalizedDistanceError);

        if (isWithAngle)
        {
            angleLR.material.color = gradient.Evaluate(normalizedAngleError);
            angleText.color = gradient.Evaluate(normalizedAngleError);
        }
    }

    public void ShowEvaluationVisualization(bool isVisible)
    {
        float alpha;
        if (isVisible) alpha = 1.0f;
        else alpha = 0.0f;

        Renderer[] children = realGO.GetComponentsInChildren<Renderer>();
        Color newColor;
        foreach (Renderer child in children)
        {
            newColor = child.material.color;
            newColor.a = alpha;
            child.material.color = newColor;
        }
    }

    public void SetGradiencyOfThisUtensil(float redColorKey)
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

    public void DestroyObjectInstance()
    {
        Destroy(virtualGO);
        Destroy(realGO);
    }
    
}
