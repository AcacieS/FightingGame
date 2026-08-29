using UnityEngine;

public class DeadHunterState : HunterState
{
    public override void Enter()
    {
        base.Enter();
        HunterWolf.Animator.SetBool("Death", true);
        HunterWolf.CanInitiateOtherState = false;
        HunterWolf.ChangeSpeed(0);
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Death", false);
    }
}
