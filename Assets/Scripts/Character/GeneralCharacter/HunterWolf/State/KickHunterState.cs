using UnityEngine;

public class KickHunterState : HunterState
{
    [SerializeField] private State JumpState;
    public override void Enter()
    {
        base.Enter();
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
    }
}
