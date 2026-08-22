using UnityEngine;

public class DefensiveAction : UtilityAction
{
    // public override float CalculateScore(Context context)
    // {
    //     float score = 0f;

    //     // Opponent is attacking
    //     if (context.TargetIsAttacking)
    //         score += 0.5f;

    //     // Opponent is close
    //     if (context.Distance <= 2f)
    //         score += 0.2f;

    //     // Low HP → become more cautious
    //     if (context.SelfHp < context.SelfMaxHp * 0.3f)
    //         score += 0.2f;

    //     // We are vulnerable
    //     // if (context.IsVulnerable)
    //     //     score += 0.1f;

    //     return Mathf.Clamp01(score);
    // }
}