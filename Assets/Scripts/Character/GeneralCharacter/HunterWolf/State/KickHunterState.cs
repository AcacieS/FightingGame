using UnityEngine;
using System.Collections;

public class KickHunterState : HunterState
{
    [SerializeField] private State JumpState;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.Animator.SetBool("Kick", true);
        HunterWolf.CanInitiateOtherState = false;
        StartCoroutine(KickCoroutine());
    }
    private IEnumerator KickCoroutine()
    {
        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }
        HunterWolf.Hit(AI.Target, 10, false);
        float speedToAddBasedOnDistance = Mathf.Sign(AI.Target.transform.position.x - transform.position.x);
        AI.Target.Rb.linearVelocity = new Vector2(speedToAddBasedOnDistance, 0).normalized * 30f;

        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }
        JumpState.Initialize(AI);
        AI.ChangeState(JumpState);
    }

    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Kick", false);
    }
}
