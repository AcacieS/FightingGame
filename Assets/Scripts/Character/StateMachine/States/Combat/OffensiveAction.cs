using UnityEngine;

public class OffensiveAction : UtilityAction
{
    // public override float CalculateScore(Context context)
    // {
    //     float score = 0f;

    //     // Close enough to engage
    //     if (context.Distance <= 2f)
    //         score += 0.3f;

    //     // Opponent is vulnerable
    //     // if (context.TargetIsVulnerable)
    //     //     score += 0.4f;

    //     // Opponent isn't currently attacking
    //     if (!context.TargetIsAttacking)
    //         score += 0.2f;

    //     // More aggressive when opponent has low HP
    //     if (context.TargetHp < context.TargetMaxHp * 0.3f)
    //         score += 0.1f;

    //     return Mathf.Clamp01(score);
    // }
}
