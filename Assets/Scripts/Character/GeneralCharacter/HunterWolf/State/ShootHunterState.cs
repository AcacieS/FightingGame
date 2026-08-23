using UnityEngine;
using System.Collections;

public class ShootHunterState : HunterState
{
    [SerializeField] private BulletHunterPool BulletPool;
    [SerializeField] private int BulletAmountToShoot;
    public override void Enter()
    {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < BulletAmountToShoot; i++)
        {
            BulletHunterWolf bullet = BulletPool.GetBullet();
            bullet.Launch((HunterWolf.GunEndPoint.transform.position - AI.Target.transform.position).normalized);
        }
    }

    public override void Exit()
    {
        base.Exit();
    }
}
