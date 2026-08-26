using UnityEngine;

public class RechargeHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 2f;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
        HunterWolf.Animator.SetBool("Recharge", true);
        HunterWolf.CanInitiateOtherState = false;
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.HasABullet = true;
        HunterWolf.Animator.SetBool("Recharge", false);
        HunterWolf.CanInitiateOtherState = true;
    }
}
