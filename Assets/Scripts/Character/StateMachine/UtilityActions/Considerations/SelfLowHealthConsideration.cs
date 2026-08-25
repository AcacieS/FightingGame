using UnityEngine;

public class SelfLowHealthConsideration : Consideration
{
    [SerializeField] private float lowHealthThreshold = 0.3f;

    protected override float Calculate(Context context)
    {
        float healthRatio =
            (float)context.SelfHp / context.SelfMaxHp;

        if (healthRatio >= lowHealthThreshold)
            return 0f;

        return 1f - Mathf.InverseLerp(
            0f,
            lowHealthThreshold,
            healthRatio
        );
    }
}