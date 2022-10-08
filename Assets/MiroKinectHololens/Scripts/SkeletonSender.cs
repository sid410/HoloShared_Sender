using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kinect = Windows.Kinect;

public class SkeletonSender : MonoBehaviour
{
    public string hololensIP = "127.0.0.1";
    public int hololensPort = 6050;

    private UDPHelper m_udpHelper;

    private ISkeletonProvider m_skeletonProvider;
    
    private void Awake()
    {
        m_udpHelper = GameObject.FindObjectOfType<UDPHelper>();

        m_skeletonProvider = GameObject.FindObjectOfType<ISkeletonProvider>();
    }

    private void Update()
    {
        NetOutMessage message = new NetOutMessage();

        Dictionary<Kinect.JointType, Vector3> jointPositions = m_skeletonProvider.GetJointPositions();

        message.WriteInt32(jointPositions.Keys.Count);
        foreach (Kinect.JointType jointType in jointPositions.Keys)
        {
            message.WriteInt32((int)jointType);

            Vector3 temp = jointPositions[jointType];
            Vector3 jointPosition = new Vector3( temp.x, temp.y, temp.z );

            // Transform kinect coordinates to marker coordinates
            jointPosition = VuforiaMarkerInfo.KinectToMarkerMatrix.MultiplyPoint(jointPosition);
            
            // Vuforia coordinates is different from unity world coordinates, such that
            // z represents the height, and the y represents depth.
            // Because of this, we interchange y and z when sending to the Hololens
            message.WriteVector3(new Vector3( jointPosition.x, jointPosition.z, jointPosition.y));
        }
        
        m_udpHelper.Send(message, hololensIP, hololensPort);
    }
}
