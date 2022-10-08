using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

/// <summary>
/// receives skeleton from udp and transform the point into dictionary
/// </summary>
public class MiroKinectSkeletonProvider : ISkeletonProvider
{
    private Dictionary<Kinect.JointType, Vector3> m_skeletonData;

    private UDPHelper m_udpHelper;

    private Dictionary<int, Kinect.JointType> m_miroNodeNumberToKinectJointTypeMap;
    private readonly int m_miroNumJoints = 15;
    private readonly bool m_miroSkeletonLeftToRight = false; //changing this value switches left to right

    protected override void Awake()
    {
        base.Awake();

        m_skeletonData = new Dictionary<Kinect.JointType, Vector3>();
        
        m_udpHelper = GameObject.FindObjectOfType<UDPHelper>();

        m_miroNodeNumberToKinectJointTypeMap = new Dictionary<int, Kinect.JointType>();
        ConstructMiroNodeToKinectJointMap();

        for (int i = 0; i < m_miroNumJoints; i++)
        {
            Kinect.JointType jointType = m_miroNodeNumberToKinectJointTypeMap[i];
            m_skeletonData.Add(jointType, Vector3.zero);
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        //when any udp message is received the method is called
        m_udpHelper.MessageReceived += UDPMessageReceived;
    }

    private void UDPMessageReceived(NetInMessage message)
    {
        /*
        int messageType = message.ReadInt32();
        if ( messageType != 9999 ) //matching with Matlab "UpdateSkeleton.m"
        {
            return;
        }
        */

        int numJoints = message.ReadInt32(); //15 joints in miro
        for( int i = 0; i < numJoints; i++ )
        {
            float jointX = message.ReadFloat(); //ReadDouble moves to the next double every time it is called
            float jointY = message.ReadFloat();
            float jointZ = message.ReadFloat();

            Kinect.JointType kinectJointType = m_miroNodeNumberToKinectJointTypeMap[i];
            Vector3 jointPosition = m_skeletonData[kinectJointType];
            jointPosition.x = jointX;
            jointPosition.y = jointY;
            jointPosition.z = jointZ;

            m_skeletonData[kinectJointType] = jointPosition;
        }

    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    public override Dictionary<Kinect.JointType, Vector3> GetJointPositions()
    {
        return m_skeletonData;
    }

    /*
     * Because Miro does not give all 25 skeleton joints, and the order of the joints
     * is not the same as the order of the joints of the original kinect
     * there is a mapping between the miro joint number and the kinect joint type
     */
    private void ConstructMiroNodeToKinectJointMap()
    {
        m_miroNodeNumberToKinectJointTypeMap.Add(0, Kinect.JointType.SpineBase);
        m_miroNodeNumberToKinectJointTypeMap.Add(1, Kinect.JointType.SpineMid);
        m_miroNodeNumberToKinectJointTypeMap.Add(2, Kinect.JointType.SpineShoulder);
        
        m_miroNodeNumberToKinectJointTypeMap.Add(3, m_miroSkeletonLeftToRight ? Kinect.JointType.ShoulderLeft : Kinect.JointType.ShoulderRight);
        m_miroNodeNumberToKinectJointTypeMap.Add(4, m_miroSkeletonLeftToRight ? Kinect.JointType.ElbowLeft : Kinect.JointType.ElbowRight);
        m_miroNodeNumberToKinectJointTypeMap.Add(5, m_miroSkeletonLeftToRight ? Kinect.JointType.WristLeft : Kinect.JointType.WristRight);

        m_miroNodeNumberToKinectJointTypeMap.Add(9, m_miroSkeletonLeftToRight ? Kinect.JointType.HipLeft : Kinect.JointType.HipRight);
        m_miroNodeNumberToKinectJointTypeMap.Add(10, m_miroSkeletonLeftToRight ? Kinect.JointType.KneeLeft : Kinect.JointType.KneeRight);
        m_miroNodeNumberToKinectJointTypeMap.Add(11, m_miroSkeletonLeftToRight ? Kinect.JointType.AnkleLeft : Kinect.JointType.AnkleRight);

        m_miroNodeNumberToKinectJointTypeMap.Add(6, m_miroSkeletonLeftToRight ? Kinect.JointType.ShoulderRight : Kinect.JointType.ShoulderLeft);
        m_miroNodeNumberToKinectJointTypeMap.Add(7, m_miroSkeletonLeftToRight ? Kinect.JointType.ElbowRight : Kinect.JointType.ElbowLeft);
        m_miroNodeNumberToKinectJointTypeMap.Add(8, m_miroSkeletonLeftToRight ? Kinect.JointType.WristRight : Kinect.JointType.WristLeft);
        
        m_miroNodeNumberToKinectJointTypeMap.Add(12, m_miroSkeletonLeftToRight ? Kinect.JointType.HipRight : Kinect.JointType.HipLeft);
        m_miroNodeNumberToKinectJointTypeMap.Add(13, m_miroSkeletonLeftToRight ? Kinect.JointType.KneeRight : Kinect.JointType.KneeLeft);
        m_miroNodeNumberToKinectJointTypeMap.Add(14, m_miroSkeletonLeftToRight ? Kinect.JointType.AnkleRight : Kinect.JointType.AnkleLeft);
    }
}
