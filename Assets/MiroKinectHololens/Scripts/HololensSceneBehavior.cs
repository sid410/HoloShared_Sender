using UnityEngine;

public class HololensSceneBehavior : MonoBehaviour
{
    public SkeletonVisualization skeletonVisualization;
    public ISkeletonProvider skeletonProvider;

    private void Update()
    {
        skeletonVisualization.SetJointPositions(skeletonProvider.GetJointPositions());
    }
}
