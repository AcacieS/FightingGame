using UnityEngine;

public class AimHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private State ShootState;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
        HunterWolf.Animator.SetBool("Aim", true);
        HunterWolf.IsAiming = true;
    }
    public override void Play()
    {
        if (timerState.IsOver())
        {
            ShootState.Initialize(AI);
            AI.ChangeState(ShootState);
        }
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.Animator.SetBool("Aim", false);
        HunterWolf.IsAiming = false;
    }
}
