using UnityEngine;

public class AimHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private State ShootState;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
    }
    public override void Update()
    {
        if (TimerState.IsOver())
        {
            ShootState.Initialize(AI);
            AI.ChangeState(ShootState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
    }
}
