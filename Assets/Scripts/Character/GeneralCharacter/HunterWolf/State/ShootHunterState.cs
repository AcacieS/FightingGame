using UnityEngine;
using System.Collections;

public class ShootHunterState : HunterState
{
    [SerializeField] private BulletHunterPool BulletPool;
    [SerializeField] private int BulletAmountToShoot;
    [SerializeField] private State StunState;
    [SerializeField] private ParticleSystem BoomParticule;
    [SerializeField] private Audio shootAudio;
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(ShootCoroutine());
        HunterWolf.Animator.SetBool("Shoot", true);
        HunterWolf.CanInitiateOtherState = false;
        AudioEventChannel.Instance.Play(shootAudio);
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }
        HunterWolf.HasABullet = false;
        for (int i = 0; i < BulletAmountToShoot; i++)
        {
            BulletHunterWolf bullet = BulletPool.GetBullet();
            bullet.transform.position = HunterWolf.GunEndPoint.position;
            bullet.Launch((AI.Target.transform.position - HunterWolf.GunEndPoint.transform.position).normalized);
        }

        BoomParticule.transform.localScale = HunterWolf.transform.localScale;
        BoomParticule.Play();

        yield return new WaitForSeconds(TimeState / 2);
        if (Context.Instance.SelfState is DeadState)
        {
            yield break;
        }
        StunState.Initialize(AI);
        AI.ChangeState(StunState);
    }

    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Shoot", false);
    }
}
