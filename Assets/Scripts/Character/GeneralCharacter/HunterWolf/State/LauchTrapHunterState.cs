using UnityEngine;
using System.Collections;

public class LauchTrapHunterState : HunterState
{
    [SerializeField] private TrapHunterPool TrapPool;
    [SerializeField] private int TrapAmountToShoot;
    public override void Enter()
    {
        StartCoroutine(ShootCoroutine());
    }

    private IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < TrapAmountToShoot; i++)
        {
            TrapHunterWolf Trap = TrapPool.GetTrap();
            Trap.Launch(new Vector2(1,2).normalized);
        }
    }
}
