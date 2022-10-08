using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Leftcenter : MonoBehaviour
{
    private extOSC.Examples.PlatformManager m_platformManager;
    public GameObject LeftFootCenterGO, LeftFootTextGO;
    private TextMesh LeftFootText;

    private void Start()
    {
        m_platformManager = GameObject.FindObjectOfType<extOSC.Examples.PlatformManager>();
        LeftFootText = LeftFootTextGO.GetComponent<TextMesh>();
    }
    
    private void Update()
    {
        LeftFootText.text = m_platformManager.Lperc.ToString() + "%";

        if (m_platformManager.Lperc > 0 && m_platformManager.ShowPlatformText)
        {
            ShowGameObject(LeftFootCenterGO, true);
            ShowGameObject(LeftFootTextGO, true);

            gameObject.transform.localPosition = new Vector3((0.6f - m_platformManager.LxPos / 100.0f)*transform.localScale.x, 0, (0.25f - m_platformManager.LyPos / 100.0f)*transform.localScale.z);
            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x, 3.0f * m_platformManager.Lperc / 100.0f, gameObject.transform.localScale.z);
            
        }
        else
        {
            ShowGameObject(LeftFootCenterGO, false);
            ShowGameObject(LeftFootTextGO, false);
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
