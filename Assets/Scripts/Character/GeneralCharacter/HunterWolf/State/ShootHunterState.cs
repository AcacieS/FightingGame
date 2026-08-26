using UnityEngine;
using System.Collections;

public class ShootHunterState : HunterState
{
    [SerializeField] private BulletHunterPool BulletPool;
    [SerializeField] private int BulletAmountToShoot;
    [SerializeField] private State IdleState;
    public override void Enter()
    {
        StartCoroutine(ShootCoroutine());
        HunterWolf.Animator.SetBool("Shoot", true);
        HunterWolf.CanInitiateOtherState = false;
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(1f);

        HunterWolf.HasABullet = false;
        for (int i = 0; i < BulletAmountToShoot; i++)
        {
            BulletHunterWolf bullet = BulletPool.GetBullet();
            bullet.Launch((HunterWolf.GunEndPoint.transform.position - AI.Target.transform.position).normalized);
        }

        AI.ChangeState(IdleState);
    }

    public override void Exit()
    {
        base.Exit();
        HunterWolf.CanInitiateOtherState = true;
        HunterWolf.Animator.SetBool("Shoot", false);
    }
}
