using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using extOSC;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct ArucoAxisMarshallingStructure
{
    public Vector3 origin;
    public float rotation;
}

public class ArucoAxisReceiver : MonoBehaviour
{
    private OSCReceiver Receiver;
    private string _address;
    private GameObject OriginGO, XaxisGO;

    private void Start()
    {
        _address = gameObject.name;
        Receiver = GameObject.FindObjectOfType<OSCReceiver>();
        Receiver.Bind(_address, UpdateTransform);

        OriginGO = transform.Find("origin").gameObject;
        XaxisGO = transform.Find("Xaxis").gameObject;
    }

    private void UpdateTransform(OSCMessage message)
    {
        byte[] bytes;

        if (!message.ToBlob(out bytes))
            return;

        var trasformStructure = OSCUtilities.ByteToStruct<ArucoAxisMarshallingStructure>(bytes);
        
        OriginGO.transform.localPosition = new Vector3(trasformStructure.origin.x, 0, trasformStructure.origin.z);

        XaxisGO.transform.localPosition = OriginGO.transform.localPosition;
        XaxisGO.transform.localEulerAngles = new Vector3(0, trasformStructure.rotation, 0);
    }

}
