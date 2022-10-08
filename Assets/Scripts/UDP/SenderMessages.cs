using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SenderMessages : MonoBehaviour
{
    void Start()
    {
        //InvokeRepeating("SendTestMessage", 3.0f, 3.0f);
        SendTestMessage();
    }

    public void SendTestMessage()
    {
        Debug.Log("Message sent");
        CustomMessages.Instance.StatusMessage();
    }

    public void SendRotateMessage()
    {
        CustomMessages.Instance.RotateMessage();
    }
    public void SendHideMessage()
    {
        CustomMessages.Instance.HideMessage();
    }
    public void SendShowMessage()
    {
        CustomMessages.Instance.ShowMessage();
    }
    public void SendNextMessage()
    {
        CustomMessages.Instance.NextMessage();
    }
    public void SendPrevMessage()
    {
        CustomMessages.Instance.PrevMessage();
    }
}
