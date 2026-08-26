using UnityEngine;

public class KickHunterState : HunterState
{
    [SerializeField] private State JumpState;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.Animator.SetBool("Kick", true);
        HunterWolf.CanInitiateOtherState = false;
    }
    public override void Play()
    {
        if (timerState.IsOver())
        {
            JumpState.Initialize(AI);
            AI.ChangeState(JumpState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Kick", false);
    }
}
