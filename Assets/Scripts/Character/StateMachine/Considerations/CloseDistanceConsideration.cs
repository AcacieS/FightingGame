using UnityEngine;

public class CloseDistanceConsideration : Consideration
{
    [SerializeField] private float maxDistance = 2f;

    protected override float Calculate(Context context)
    {
        if (context.Distance <= maxDistance)
            return 1f;

        return 0f;
    }
}