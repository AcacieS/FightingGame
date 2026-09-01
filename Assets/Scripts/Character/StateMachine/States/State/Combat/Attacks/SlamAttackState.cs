using UnityEngine;

public class SlamAttackState : AttackState
{
    [SerializeField] private CooldownRequirement coolDownRequirement;
    public override void Enter()
    {
        base.Enter();
        coolDownRequirement.Initialize();
        if (Context.Target.IsOnGround)
        {
            AttackPlayer();
        }
    }
    public override void Play()
    {
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestDecision();
        }
    }
}
