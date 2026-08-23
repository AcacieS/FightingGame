using UnityEngine;

public class TargetAttackingConsideration : Consideration
{
    [SerializeField] private bool TargetIsAttackingCondition = true;
    protected override float Calculate(Context context)
    {
        if (TargetIsAttackingCondition)
        {
            return context.TargetIsAttacking ? 1f : 0f;
        }
        
        return !context.TargetIsAttacking ? 1f : 0f;
    }
}