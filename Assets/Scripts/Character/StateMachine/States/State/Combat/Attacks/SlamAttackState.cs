using UnityEngine;

public class SlamAttackState : MeleeAttackState
{
    [SerializeField] private CooldownRequirement coolDownRequirement;
    public override void Enter()
    {
        base.Enter();
        coolDownRequirement.Initialize();
    }
    protected override void OnAttackHit()
    {
        base.OnAttackHit();
        if (Context.Target is Player player)
        {
            player.Stun();
        }
        //TODO: CHECK SUCCESSFULLY
        AttackResult = AttackResult.Success;
    }
}
