using UnityEngine;

public class JumpHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 8f;
    [SerializeField] private float jumpForce = 7f;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
        HunterWolf.Jump(jumpForce);
        HunterWolf.Animator.SetBool("Jump", true);
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.Animator.SetBool("Jump", false);
        HunterWolf.CanInitiateOtherState = true;
    }
}
