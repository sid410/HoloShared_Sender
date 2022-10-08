using HoloToolkit.Unity;
using System.Collections.Generic;
using UnityEngine;

public class CustomMessages : Singleton<CustomMessages>
{
    public string destinationIP = "127.0.0.1";
    public int destinationPort = 11000;
    public string hololensIP = "127.0.0.1";
    public int hololensPort = 11000;


    UDPHelper udpHelper;

    protected override void Awake()
    {
        udpHelper = GameObject.FindObjectOfType<UDPHelper>();
    }

    void Start()
    {
        InitializeMessageHandlers();
    }

    void InitializeMessageHandlers()
    {
    }

    public void StatusMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Im Alive!!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void RaycastHitposMessage(Vector3 _ray)
    { 
        // add what to do with raypos here, call in PlaceonMesh.Place
    }

    public void SendTransformData(Vector3 pos, Vector3 rot, Vector3 scale)
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteVector3(pos);
            msg.WriteVector3(rot);
            msg.WriteVector3(scale);
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void SendTextureData(int camW, int camH, int chunkCount, byte[] data)
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteInt32(camW);
            msg.WriteInt32(camH);
            msg.WriteInt32(chunkCount);
            msg.WriteInt32(data.Length);
            msg.WriteBytes(data);

            udpHelper.Send(msg, hololensIP, hololensPort);

        }
        else Debug.Log("no server connection!");
    }

    public void RotateMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Rotate Cylinder clicked!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void HideMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Hide Cylinder clicked!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void ShowMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Show Cylinder clicked!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void NextMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Next Cylinder clicked!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }

    public void PrevMessage()
    {
        if (this.udpHelper != null)
        {
            NetOutMessage msg = new NetOutMessage();
            msg.WriteString("Prev Cylinder clicked!");
            udpHelper.Send(msg, destinationIP, destinationPort);
        }
        else Debug.Log("no server connection!");
    }


    //public void SendBodyData(Kinect.Body _body)
    //{
    //    Debug.Log("sending body info");
    //    // If we are connected to a session, broadcast our head info
    //    if (this.udpHelper != null)
    //    {
    //        // Create an outgoing network message to contain all the info we want to send
    //        NetOutMessage msg = new NetOutMessage();
    //        msg.WriteInt32(_body.Joints.Count);
    //        Debug.Log("" + _body.Joints.Count);
    //        for (Kinect.JointType jt = Kinect.JointType.SpineBase; jt <= Kinect.JointType.ThumbRight; jt++)
    //        {
    //            if (_body.Joints.ContainsKey(jt))
    //            {
    //                Vector3 jointPos = new Vector3(_body.Joints[jt].Position.X, _body.Joints[jt].Position.Y, _body.Joints[jt].Position.Z);
    //                jointPos = bodyInfo.transformToMarker.MultiplyPoint3x4(jointPos);
    //                msg.WriteInt32((int)jt);
    //                msg.WriteFloat(jointPos.x);
    //                msg.WriteFloat(jointPos.z);
    //                msg.WriteFloat(jointPos.y);
    //                //msg.WriteInt32((int)_body.Joints[jt].TrackingState);
    //                if (jt == Kinect.JointType.SpineBase)
    //                {
    //                    Debug.Log("Joint ID: " + (int)jt + " X: " + jointPos.x + "Y: " + jointPos.y +
    //                        " Z: " + jointPos.z + " ST: " + _body.Joints[jt].TrackingState);
    //                }
    //            }
    //        }

    //        udpHelper.Send( msg, destinationIP1, destinationPort );
    //        udpHelper.Send(msg, destinationIP2, destinationPort);
    //    }
    //    else
    //    {
    //        Debug.Log("no server connection!");
    //    }
    //}
}