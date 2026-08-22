using UnityEngine;

public class MaxDistanceRequirement : Requirement
{
    [SerializeField] private float maxDistance = 2f;

    public override bool IsMet(Context context)
    {
        return context.Distance <= maxDistance;
    }
}