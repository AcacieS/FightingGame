using UnityEngine;

public class DistanceGraduateConsideration : Consideration
{
    [SerializeField] private float idealDistance = 2f;
    [SerializeField] private float maxDistance = 8f;

    protected override float Calculate(Context context)
    {
        if (context.DistanceX >= maxDistance)
            return 0f;

        if (context.DistanceX <= idealDistance)
            return 1f;

        return 1f - Mathf.InverseLerp(
            idealDistance,
            maxDistance,
            context.DistanceX
        );
    }
}