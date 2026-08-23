using UnityEngine;

public class DistanceConsideration : Consideration
{
    public enum Mode
    {
        Close,
        Far,
        Curve,
        Range
    }

    [SerializeField] private Mode mode;

    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;

    [SerializeField]
    private AnimationCurve curve = AnimationCurve.Linear(0f, 1f, 1f, 0f);

    protected override float Calculate(Context context)
    {
        float distance = context.Distance;

        switch (mode)
        {
            case Mode.Close:
                return distance <= maxDistance ? 1f : 0f;

            case Mode.Far:
                return distance >= minDistance ? 1f : 0f;

            case Mode.Curve:
                return CalculateCurve(distance);

            case Mode.Range:
                return distance >= minDistance &&
                       distance <= maxDistance
                    ? 1f
                    : 0f;

            default:
                return 0f;
        }
    }
    //TODO: Not finished
    private float CalculateCurve(float distance)
    {
        if (maxDistance <= minDistance)
            return 0f;

        float normalizedDistance = Mathf.InverseLerp(
            minDistance,
            maxDistance,
            distance
        );

        return Mathf.Clamp01(curve.Evaluate(normalizedDistance));
    }
}