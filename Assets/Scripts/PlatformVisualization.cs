using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformVisualization : MonoBehaviour
{
    public extOSC.Examples.PlatformManager m_platformManager;
    public GameObject platformPlaneGO;

    void Update()
    {
        UpdatePlatformVisualization();
    }

    private void UpdatePlatformVisualization()
    {
        if (m_platformManager == null || platformPlaneGO == null || platformPlaneGO.GetComponent<Renderer>().enabled == false)
        {
            return;
        }

        Dictionary<(int, int), int> platformData = m_platformManager.GetPlatformData();
        Texture2D texture = new Texture2D(120, 50);
        platformPlaneGO.GetComponent<Renderer>().material.mainTexture = texture;

        var values = 0;

        for (var x = 0; x < 120; x++)
        {
            for (var y = 0; y < 50; y++)
            {
                if (platformData[(x, y)] != 0)
                {
                    texture.SetPixel(x, y, new Color(2.0f * (platformData[(x, y)] / 245.0f), 2.0f * ((245.0f - platformData[(x, y)]) / 245.0f), 0));
                    values = 1;
                }
                else
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 0));
                }
            }
        }
        if (values == 0)
        {
            for (var x = 0; x < 120; x++)
            {
                for (var y = 0; y < 50; y++)
                {
                    texture.SetPixel(x, y, new Color(0, 0, 0, 1));
                }
            }
        }

        texture.Apply();
    }
}
