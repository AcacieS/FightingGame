using UnityEngine;

public abstract class AttackState : ActionState
{
    [Header("Attack State")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected bool _doesStun;
    [SerializeField] protected bool _doesInterrupt;
    [ReadOnly, SerializeField] protected AttackResult attackResult;
    public AttackResult AttackResult => attackResult;
    public override void Enter()
    {
        attackResult = AttackResult.None;
        base.Enter();
    }
}