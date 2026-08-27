using UnityEngine;

public abstract class AttackState : ActionState
{
    [Header("Attack State")]
    [SerializeField] protected int damage = 10;
    [ReadOnly, SerializeField] protected AttackResult attackResult;
    public AttackResult AttackResult => attackResult;
    public override void Enter()
    {
        attackResult = AttackResult.None;
        base.Enter();
    }
}