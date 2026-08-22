public class TargetNotAttackingConsideration : Consideration
{
    protected override float Calculate(Context context)
    {
        return context.TargetIsAttacking ? 0f : 1f;
    }
}