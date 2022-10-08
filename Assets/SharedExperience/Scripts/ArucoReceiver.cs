using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using extOSC;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ArucoMarshallingStructure
{
    public Vector3 PosValue;
    public Vector3 RotValue;
}

public class ArucoReceiver : MonoBehaviour
{
    private OSCReceiver Receiver;
    private string _address;

    private void Start()
    {
        _address = gameObject.name;
        Receiver = GameObject.FindObjectOfType<OSCReceiver>();
        Receiver.Bind(_address, UpdateTransform);
    }

    private void UpdateTransform(OSCMessage message)
    {
        byte[] bytes;

        if (!message.ToBlob(out bytes))
            return;

        var trasformStructure = OSCUtilities.ByteToStruct<ArucoMarshallingStructure>(bytes);

        //transform.localPosition = trasformStructure.PosValue;
        //transform.localEulerAngles = trasformStructure.RotValue;
        transform.localPosition = new Vector3(trasformStructure.PosValue.x, 0, trasformStructure.PosValue.z);
        transform.localRotation = Quaternion.identity;

    }

}
