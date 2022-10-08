using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockRotationXYZ : MonoBehaviour
{
    [SerializeField]
    [Tooltip("set to true to lock axis")]
    private bool allAxis, xzAxis;

    private void Update()
    {
        if (allAxis)
        {
            this.transform.localRotation = Quaternion.identity;
        }
        if (xzAxis)
        {
            this.transform.localEulerAngles = new Vector3(0, this.transform.localEulerAngles.y, 0);
        }
    }
}
