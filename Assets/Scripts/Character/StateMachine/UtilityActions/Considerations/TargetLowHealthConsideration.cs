using UnityEngine;

public class TargetLowHealthConsideration : Consideration
{
    [SerializeField] private float lowHealthThreshold = 0.3f;

    protected override float Calculate(Context context)
    {
        float healthRatio =
            (float)context.TargetHp / context.TargetMaxHp;

        if (healthRatio >= lowHealthThreshold)
            return 0f;

        return 1f - Mathf.InverseLerp(
            0f,
            lowHealthThreshold,
            healthRatio
        );
    }
}