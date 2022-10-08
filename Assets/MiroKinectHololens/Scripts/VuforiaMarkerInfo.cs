using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VuforiaMarkerInfo
{
    public static Matrix4x4 KinectToMarkerMatrix
    {
        get
        {
            Matrix4x4 transformToMarker = Matrix4x4.identity;

            /*
             * Supply values for this matrix using the KinectV2Localization project
             */
            transformToMarker.SetRow(0, new Vector4(-0.983105f, -0.0277128f, -0.180931f, 0.562667f));
            transformToMarker.SetRow(1, new Vector4(-0.049599f, 0.991823f, 0.117586f, 0.161206f));
            transformToMarker.SetRow(2, new Vector4(0.176192f, 0.124573f, -0.976441f, 2.2181f));

            return transformToMarker;
        }
    }

    public static Matrix4x4 MarkerToKinectMatrix
    {
        get
        {
            return KinectToMarkerMatrix.inverse;
        }
    }
}
