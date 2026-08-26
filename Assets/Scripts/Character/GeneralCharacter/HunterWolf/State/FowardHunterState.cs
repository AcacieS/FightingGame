using UnityEngine;

public class FowardHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 4f;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed, 1);
        HunterWolf.Animator.SetBool("Foward", true);
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
        HunterWolf.Animator.SetBool("Foward", false);
    }
}
