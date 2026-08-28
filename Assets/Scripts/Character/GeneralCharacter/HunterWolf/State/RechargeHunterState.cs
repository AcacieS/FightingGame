using UnityEngine;

public class RechargeHunterState : HunterState
{
    [SerializeField] private State IdleState;
    [SerializeField] private float moveSpeed = 2f;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
        HunterWolf.Animator.SetBool("Recharge", true);
        HunterWolf.IsRecharging = true;
        HunterWolf.CanInitiateOtherState = false;
    }
    public override void Play()
    {
        if (timerState.IsOver())
        {
            IdleState.Initialize(AI);
            AI.ChangeState(IdleState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.TimerWaitBetweenState.Restart();
        HunterWolf.HasABullet = true;
        HunterWolf.Animator.SetBool("Recharge", false);
        HunterWolf.IsRecharging = false;
        HunterWolf.CanInitiateOtherState = true;
    }
}
