public class TargetAttackingConsideration : Consideration
{
    protected override float Calculate(Context context)
    {
        return context.TargetIsAttacking ? 1f : 0f;
    }
}