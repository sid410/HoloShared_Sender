using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using extOSC.Examples;
using extOSC;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
public struct TransformMarshallingStructure
{
    public Vector3 PosValue;
    public Quaternion RotValue;
}

public class TransformSender : MonoBehaviour
{
    private OSCTransmitter Transmitter;
    private string _address;

    private Vector3 newPos, oldPos;
    private float threshold = 0.00001f;

    private void Start()
    {
        _address = GetFullName(this.gameObject);
        Transmitter = GameObject.FindObjectOfType<OSCTransmitter>();

        oldPos = this.transform.localPosition;
        newPos = this.transform.localPosition;
    }
    
    private void Update()
    {
        newPos = this.transform.localPosition;
        Vector3 delta = newPos - oldPos;
        oldPos = this.transform.localPosition;

        if (delta.sqrMagnitude > threshold) SendUpdatedTransform();
    }

    private static string GetFullName(GameObject go)
    {
        string name = go.name;
        while (go.transform.parent != null)
        {

            go = go.transform.parent.gameObject;
            name = go.name + "/" + name;
        }
        return name;
    }

    public void SendUpdatedTransform()
    {
        var message = new OSCMessage(_address);
        var trasformStructure = new TransformMarshallingStructure();

        trasformStructure.PosValue = transform.localPosition;
        trasformStructure.RotValue = transform.localRotation;

        var bytes = OSCUtilities.StructToByte(trasformStructure);
        message.AddValue(OSCValue.Blob(bytes));
        
        Transmitter.Send(message);
    }
}
