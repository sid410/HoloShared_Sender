using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Windows.Kinect;

public class TestSkeletonProvider : MonoBehaviour
{
    public string testFile = "test";
    private string[] m_lines;

    private int m_currentIndex;

    private UDPHelper m_udpHelper;

    private void Awake()
    {
        m_udpHelper = GameObject.FindObjectOfType<UDPHelper>();
    }

    private void Start()
    {
        m_currentIndex = 0;

        TextAsset textAsset = Resources.Load<TextAsset>(testFile);
        m_lines = textAsset.text.Split('\n');
    }

    private void Update()
    {
        NetOutMessage message = new NetOutMessage();
        message.WriteInt32(9999);
        message.WriteInt32(15);

        string current = m_lines[m_currentIndex];
        string[] split = current.Split(',');
        for (int i = 0; i < split.Length; i++)
        {
            if (!string.IsNullOrEmpty(split[i].Trim()))
            {
                message.WriteDouble(double.Parse(split[i].Trim()));
            }
        }
        
        m_udpHelper.Send(message, "127.0.0.1", 6050);

        m_currentIndex = (m_currentIndex + 1) % m_lines.Length;
    }
}
