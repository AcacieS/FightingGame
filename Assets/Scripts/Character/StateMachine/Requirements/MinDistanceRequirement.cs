using UnityEngine;

public class MinDistanceRequirement : Requirement
{
    [SerializeField] private float minDistance = 2f;

    public override bool IsMet(Context context)
    {
        return context.Distance >= minDistance;
    }
}