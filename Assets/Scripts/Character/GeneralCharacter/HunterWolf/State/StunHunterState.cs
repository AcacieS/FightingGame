using UnityEngine;

public class StunHunterState : HunterState
{
    [SerializeField] private State IdleState;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.Animator.SetBool("Stun", true);
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
        HunterWolf.TimerWaitBetweenState.Restart();
        HunterWolf.Animator.SetBool("Stun", true);
        HunterWolf.CanInitiateOtherState = true;
    }
}
