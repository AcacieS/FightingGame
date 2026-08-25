using UnityEngine;

public class DistanceRequirement : Requirement
{
    public enum Mode
    {
        Close,
        Far,
        Range
    }

    [SerializeField] private Mode mode;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    public override bool IsMet(Context context)
    {
        float distance = context.Distance;

        switch (mode)
        {
            case Mode.Close:
                return distance <= maxDistance;

            case Mode.Far:
                return distance >= minDistance;

            case Mode.Range:
                return distance >= minDistance &&
                       distance <= maxDistance;

            default:
                return false;
        }
    }
}