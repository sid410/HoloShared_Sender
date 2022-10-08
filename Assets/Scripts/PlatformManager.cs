using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using M2MqttUnity;

namespace extOSC.Examples
{
    public class PlatformManager : MonoBehaviour
    {
        public BaseClient baseClient;

        private int _width = 120, _height = 50, _Rperc, _Lperc, _RxPos, _RyPos, _LxPos, _LyPos;

        public int Rperc { get { return _Rperc; } }
        public int Lperc { get { return _Lperc; } }
        public int RxPos { get { return _RxPos; } }
        public int RyPos { get { return _RyPos; } }
        public int LxPos { get { return _LxPos; } }
        public int LyPos { get { return _LyPos; } }

        public string Address1 = "/test/1";
        public string Address2 = "/test/2";

        private Dictionary<(int, int), int> m_platformData;
        public GameObject platformPlaneGO;

        private bool _ShowPlatformText = false;
        public bool ShowPlatformText { get { return _ShowPlatformText; } }


        [Header("OSC Settings")]
        public OSCReceiver Receiver;

        private void OnEnable()
        {
            baseClient.RegisterTopicHandler("M2MQTT/ShowPlatformVisualization", HandlePlatformVisualization);
        }

        private void OnDisable()
        {
            baseClient.UnregisterTopicHandler("M2MQTT/ShowPlatformVisualization", HandlePlatformVisualization);
        }

        private void HandlePlatformVisualization(string topic, string message)
        {
            if (topic == "M2MQTT/ShowPlatformVisualization" && message == "true") ShowPlatformVisualization(true);
            if (topic == "M2MQTT/ShowPlatformVisualization" && message == "false") ShowPlatformVisualization(false);
        }

        private void ShowPlatformVisualization(bool isVisible)
        {
            float alpha;
            if (isVisible) alpha = 1.0f;
            else alpha = 0.0f;

            Renderer[] children = platformPlaneGO.GetComponentsInChildren<Renderer>();
            Color newColor;
            foreach (Renderer child in children)
            {
                newColor = child.material.color;
                newColor.a = alpha;
                child.material.color = newColor;
            }

            _ShowPlatformText = isVisible;
        }

        protected virtual void Start()
        {
            m_platformData = new Dictionary<(int, int), int>();

            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    m_platformData[(x, y)] = 0;
                }
            }

            Receiver.Bind(Address1, ReceivedMessage1);
            Receiver.Bind(Address2, ReceivedMessage2);

            ShowPlatformVisualization(false);
        }

        private void ReceivedMessage1(OSCMessage message)
        {
            for (var x = 0; x < _width; x++)
            {
                for (var y = 0; y < _height; y++)
                {
                    var id = _height * x + y;
                    var val = message.Values[0].BlobValue;
                    //y = 49 - y;
                    m_platformData[(x, y)] = val[id];
                }
            }
        }
        private void ReceivedMessage2(OSCMessage message)
        {

            _Rperc = message.Values[0].IntValue;
            _Lperc = message.Values[1].IntValue;
            _RxPos = message.Values[2].IntValue;
            _RyPos = message.Values[3].IntValue;
            _LxPos = message.Values[4].IntValue;
            _LyPos = message.Values[5].IntValue;

        }

        public Dictionary<(int, int), int> GetPlatformData()
        {
            return m_platformData;
        }
    }
}
