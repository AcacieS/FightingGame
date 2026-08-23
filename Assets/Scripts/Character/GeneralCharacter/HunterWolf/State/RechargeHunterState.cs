using UnityEngine;

public class RechargeHunterState : HunterState
{
    [SerializeField] private float moveSpeed = 2f;
    public override void Enter()
    {
        base.Enter();
        HunterWolf.ChangeSpeed(moveSpeed);
    }
    public override void Exit()
    {
        base.Exit();
        HunterWolf.ChangeSpeed(0);
    }
}
