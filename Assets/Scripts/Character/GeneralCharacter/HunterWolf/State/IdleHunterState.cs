using UnityEngine;

public class IdleHunterWolf : HunterState
{
    public override void Enter()
    {
        base.Enter();
        HunterWolf.Animator.SetBool("Idle", true);
    }
    public override void Play()
    {

    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Idle", false);
    }
}
