using UnityEngine;
using System.Collections;

public class ShootHunterState : HunterState
{
    [SerializeField] private BulletHunterPool BulletPool;
    [SerializeField] private int BulletAmountToShoot;
    [SerializeField] private State StunState;
    public override void Enter()
    {
        base.Enter();
        StartCoroutine(ShootCoroutine());
        HunterWolf.Animator.SetBool("Shoot", true);
        HunterWolf.CanInitiateOtherState = false;
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(TimeState / 2);

        HunterWolf.HasABullet = false;
        for (int i = 0; i < BulletAmountToShoot; i++)
        {
            BulletHunterWolf bullet = BulletPool.GetBullet();
            bullet.Launch((AI.Target.transform.position - HunterWolf.GunEndPoint.transform.position).normalized);
        }

        yield return new WaitForSeconds(TimeState / 2);

        AI.ChangeState(StunState);
    }

    public override void Exit()
    {
        base.Exit();
        HunterWolf.Animator.SetBool("Shoot", false);
    }
}
