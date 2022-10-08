using UnityEngine;
using System.Collections;
using Windows.Kinect;

public class ColorSourceView : MonoBehaviour
{
    public ColorSourceManager colorManager;
    
    void Start ()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
    }
    
    void LateUpdate()
    {
        if (colorManager == null)
        {
            return;
        }
        
        Texture2D tex = colorManager.GetColorTexture();
        gameObject.GetComponent<Renderer>().material.mainTexture = tex;
    }
}
