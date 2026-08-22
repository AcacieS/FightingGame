using UnityEngine;

public class AttackAction : UtilityAction
{
    [SerializeField] private State targetState;

    // public override float CalculateScore(Context context)
    // {
    //     if (context.Distance > 2f)
    //         return 0f;

    //     return 0.8f;
    // }
}