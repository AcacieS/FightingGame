using UnityEngine;
using System.Collections;

public class LauchTrapHunterState : HunterState
{
    [SerializeField] private TrapHunterPool TrapPool;
    [SerializeField] private State IdleState;
    [SerializeField] private int TrapAmountToShoot;
    [SerializeField] private int TrapSpeed = 15;
    [SerializeField] private int TrapSpeedEach = 5;
    [SerializeField] private Audio launchTrapSFX;
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(ShootCoroutine());
        HunterWolf.Animator.SetBool("LauchTrap", true);
        AudioEventChannel.Instance.Play(launchTrapSFX);
        HunterWolf.CanInitiateOtherState = false;
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }

        float randomSpeed = Random.Range(.8f, 1.2f);
        float speedToAddBasedOnDistance = Mathf.Abs(AI.Target.transform.position.x - transform.position.x) * .2f;
        for (int i = 0; i < TrapAmountToShoot; i++)
        {
            TrapHunterWolf Trap = TrapPool.GetTrap();
            Trap.transform.position = HunterWolf.LauchEndPoint.position;
            Trap.Launch(new Vector2(-HunterWolf.transform.localScale.x, 3).normalized * TrapSpeed * randomSpeed + Vector2.left * (TrapSpeedEach * i * HunterWolf.transform.localScale.x) + Vector2.left * speedToAddBasedOnDistance * HunterWolf.transform.localScale.x);
        }

        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }
        IdleState.Initialize(AI);
        AI.ChangeState(IdleState);
    }

    public override void Exit()
    {
        base.Exit();
        HunterWolf.TimerWaitBetweenState.Restart();
        HunterWolf.Animator.SetBool("LauchTrap", false);
        HunterWolf.CanInitiateOtherState = true;
    }
}
