using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorVisualization : MonoBehaviour
{
    public ISkeletonProvider skeletonProvider;
    public GameObject mirrorPlaneGO;


    void Update()
    {
        UpdateMirrorVisualization();
    }

    private void UpdateMirrorVisualization()
    {
        if (skeletonProvider == null || mirrorPlaneGO == null)
        {
            return;
        }

        Dictionary<Windows.Kinect.JointType, Vector3> jointPositions = skeletonProvider.GetJointPositions();
        Texture2D texture = new Texture2D(100, 100);
        mirrorPlaneGO.GetComponent<Renderer>().material.mainTexture = texture;

        foreach (Windows.Kinect.JointType joint in jointPositions.Keys)
        {
            if (jointPositions.TryGetValue(joint, out Vector3 value))
            {
                texture.SetPixel((int)(value.x * 50), (int)(value.y * 50)-50, new Color(255, 0, 0));
            }
        }

        

        //for (var x = 0; x < 100; x++)
        //{
        //    for (var y = 0; y < 100; y++)
        //    {
        //        if (jointPositions.TryGetValue(Windows.Kinect.JointType.SpineBase, out Vector3 value))
        //        {
        //            texture.SetPixel(x, y, new Color(value.x, value.y, value.z));
        //        }
                
        //    }
        //}

        texture.Apply();
    }
    
}
