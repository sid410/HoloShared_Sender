using System.Collections;
using System.Collections.Generic;

using Kinect = Windows.Kinect;

using UnityEngine;

public abstract class ISkeletonProvider : MonoBehaviour
{
    public abstract Dictionary<Kinect.JointType, Vector3> GetJointPositions();

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
    }

    protected virtual void Update()
    {
    }

    protected virtual void OnEnable()
    {
    }

    protected virtual void OnDisable()
    {
    }
}
