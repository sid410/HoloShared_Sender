using UnityEngine;

public class KinectSceneBehavior : MonoBehaviour
{
    public SkeletonVisualization skeletonVisualization;
    public ISkeletonProvider skeletonProvider;

    private void Update()
    {
        skeletonVisualization.SetJointPositions(skeletonProvider.GetJointPositions());
    }
}
