using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rightcenter : MonoBehaviour
{
    private extOSC.Examples.PlatformManager m_platformManager;
    public GameObject RightFootCenterGO, RightFootTextGO;
    private TextMesh RightFootText;

    private void Start()
    {
        m_platformManager = GameObject.FindObjectOfType<extOSC.Examples.PlatformManager>();
        RightFootText = RightFootTextGO.GetComponent<TextMesh>();
    }
    
    private void Update()
    {
        RightFootText.text = m_platformManager.Rperc.ToString() + "%";

        if (m_platformManager.Rperc > 0 && m_platformManager.ShowPlatformText)
        {
            ShowGameObject(RightFootCenterGO, true);
            ShowGameObject(RightFootTextGO, true);

            gameObject.transform.localPosition = new Vector3((0.6f - m_platformManager.RxPos / 100.0f)*transform.localScale.x, 0, (0.25f - m_platformManager.RyPos / 100.0f)*transform.localScale.z);
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, 3.0f * m_platformManager.Rperc / 100.0f, gameObject.transform.localScale.z);
            
        }
        else
        {
            ShowGameObject(RightFootCenterGO, false);
            ShowGameObject(RightFootTextGO, false);
        }
    }

    private void ShowGameObject(GameObject GO, bool show)
    {
        float alpha;
        if (show) alpha = 1.0f;
        else alpha = 0.0f;

        Color newColor;
        Renderer renderer = GO.GetComponent<Renderer>();
        newColor = renderer.material.color;
        newColor.a = alpha;
        renderer.material.color = newColor;
    }
}
