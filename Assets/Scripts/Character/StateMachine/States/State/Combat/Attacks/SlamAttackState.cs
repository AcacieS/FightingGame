using UnityEngine;

public class SlamAttackState : MeleeAttackState
{
    protected override void OnAttackHit()
    {
        base.OnAttackHit();
        if (Context.Target is Player player)
        {
            player.Stun();
        }
        //TODO: CHECK SUCCESSFULLY
        attackResult = AttackResult.Success;
    }
}
