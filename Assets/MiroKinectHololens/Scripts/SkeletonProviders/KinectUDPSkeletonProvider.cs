using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Kinect = Windows.Kinect;
using extOSC;

public class KinectUDPSkeletonProvider : ISkeletonProvider
{
    //private UDPHelper m_udpHelper;
    private OSCReceiver Receiver;

    private Dictionary<Kinect.JointType, Vector3> m_jointPositions;

    protected override void Awake()
    {
        base.Awake();

        //m_udpHelper = GameObject.FindObjectOfType<UDPHelper>();
        

        m_jointPositions = new Dictionary<Kinect.JointType, Vector3>();

        Receiver = GameObject.FindObjectOfType<OSCReceiver>();
        Receiver.Bind("/kinectdata", ReceivedMessageSkeleton);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        
        //m_udpHelper.MessageReceived += UDPMessageReceived;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        //m_udpHelper.MessageReceived -= UDPMessageReceived;
    }

    //private void UDPMessageReceived(NetInMessage message)
    //{
    //    //Debug.Log("Skeleton received");
    //    int numJoints = message.ReadInt32();
    //    for( int i = 0; i < numJoints; i++)
    //    {
    //        Kinect.JointType jointType = (Kinect.JointType)message.ReadInt32();
    //        //Vector3 jointPosition = message.ReadVector3();
    //        Vector3 jointPosition;
    //        jointPosition.x = message.ReadFloat();
    //        jointPosition.y = message.ReadFloat();
    //        jointPosition.z = message.ReadFloat();

    //        m_jointPositions[jointType] = jointPosition;
    //    }
    //}

    private void ReceivedMessageSkeleton(OSCMessage message)
    {
        int numJoints = message.Values[0].IntValue;
        for (int i = 0; i < numJoints; i++)
        {
            Kinect.JointType jointType = (Kinect.JointType)message.Values[(4 * i) + 1].IntValue;
            Vector3 jointPosition;
            jointPosition.x = message.Values[(4 * i) + 2].FloatValue;
            jointPosition.y = message.Values[(4 * i) + 3].FloatValue;
            jointPosition.z = message.Values[(4 * i) + 4].FloatValue;

            m_jointPositions[jointType] = jointPosition;
        }
    }

    public override Dictionary<Kinect.JointType, Vector3> GetJointPositions()
    {
        return m_jointPositions;
    }
}
