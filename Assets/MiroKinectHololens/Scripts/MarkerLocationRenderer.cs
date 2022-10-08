using UnityEngine;

using Windows.Kinect;

public class MarkerLocationRenderer : MonoBehaviour
{
    public ColorSourceManager colorManager;

    private CoordinateMapper mapper;

    private void Start()
    {
        gameObject.GetComponent<Renderer>().material.SetTextureScale("_MainTex", new Vector2(-1, 1));
        mapper = KinectSensor.GetDefault().CoordinateMapper;

        Debug.Log(VuforiaMarkerInfo.KinectToMarkerMatrix);
    }

    private void LateUpdate()
    {
        Texture2D tex = colorManager.GetColorTexture();

        float width = 0.38f / 2f;
        float height = 0.265f / 2f;
        CameraSpacePoint pointTL = new CameraSpacePoint();

        Vector3 pointKinect = VuforiaMarkerInfo.MarkerToKinectMatrix.MultiplyPoint3x4(new Vector3(-width, -height, 0f));
        pointTL.X = pointKinect.x;
        pointTL.Y = pointKinect.y;
        pointTL.Z = pointKinect.z;

        CameraSpacePoint pointTR = new CameraSpacePoint();
        pointKinect = VuforiaMarkerInfo.MarkerToKinectMatrix.MultiplyPoint3x4(new Vector3(width, -height, 0f));
        pointTR.X = pointKinect.x;
        pointTR.Y = pointKinect.y;
        pointTR.Z = pointKinect.z;

        CameraSpacePoint pointBR = new CameraSpacePoint();
        pointKinect = VuforiaMarkerInfo.MarkerToKinectMatrix.MultiplyPoint3x4(new Vector3(width, height, 0f));
        pointBR.X = pointKinect.x;
        pointBR.Y = pointKinect.y;
        pointBR.Z = pointKinect.z;

        CameraSpacePoint pointBL = new CameraSpacePoint();
        pointKinect = VuforiaMarkerInfo.MarkerToKinectMatrix.MultiplyPoint3x4(new Vector3(-width, height, 0f));
        pointBL.X = pointKinect.x;
        pointBL.Y = pointKinect.y;
        pointBL.Z = pointKinect.z;


        ColorSpacePoint cTL = mapper.MapCameraPointToColorSpace(pointTL);
        ColorSpacePoint cTR = mapper.MapCameraPointToColorSpace(pointTR);
        ColorSpacePoint cBR = mapper.MapCameraPointToColorSpace(pointBR);
        ColorSpacePoint cBL = mapper.MapCameraPointToColorSpace(pointBL);

        DrawLine(tex, (int)cTL.X, (int)cTL.Y, (int)cTR.X, (int)cTR.Y, Color.red);
        DrawLine(tex, (int)cBL.X, (int)cBL.Y, (int)cBR.X, (int)cBR.Y, Color.red);
        DrawLine(tex, (int)cBL.X, (int)cBL.Y, (int)cTL.X, (int)cTL.Y, Color.red);
        DrawLine(tex, (int)cTR.X, (int)cTR.Y, (int)cBR.X, (int)cBR.Y, Color.red);
        tex.Apply();
    }

    private void DrawLine(Texture2D tex, int x0, int y0, int x1, int y1, Color col)
    {
        int dy = (int)(y1 - y0);
        int dx = (int)(x1 - x0);
        int stepx, stepy;

        if (dy < 0) { dy = -dy; stepy = -1; }
        else { stepy = 1; }
        if (dx < 0) { dx = -dx; stepx = -1; }
        else { stepx = 1; }
        dy <<= 1;
        dx <<= 1;

        float fraction = 0;

        tex.SetPixel(x0, y0, col);
        if (dx > dy)
        {
            fraction = dy - (dx >> 1);
            while (Mathf.Abs(x0 - x1) > 1)
            {
                if (fraction >= 0)
                {
                    y0 += stepy;
                    fraction -= dx;
                }
                x0 += stepx;
                fraction += dy;
                for (int i = -3; i < 3; ++i)
                {
                    for (int j = -3; j < 3; ++j)
                    {
                        tex.SetPixel(x0 + i, y0 + j, col);
                    }
                }

            }
        }
        else
        {
            fraction = dx - (dy >> 1);
            while (Mathf.Abs(y0 - y1) > 1)
            {
                if (fraction >= 0)
                {
                    x0 += stepx;
                    fraction -= dy;
                }
                y0 += stepy;
                fraction += dx;
                for (int i = -3; i < 3; ++i)
                {
                    for (int j = -3; j < 3; ++j)
                    {
                        tex.SetPixel(x0 + i, y0 + j, col);
                    }
                }
            }
        }
    }
}
