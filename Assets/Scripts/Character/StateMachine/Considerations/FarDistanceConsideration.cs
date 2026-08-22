using UnityEngine;

public class FarDistanceConsideration : Consideration
{
    [SerializeField] private float maxDistance = 2f;

    protected override float Calculate(Context context)
    {
        if (context.Distance > maxDistance)
            return 0f;

        return 1f;
    }
}