using UnityEngine;

public abstract class AttackState : ActionState
{
    [Header("Attack State")]
    [SerializeField] protected int damage = 10;
    [SerializeField] protected bool _doesStun;
    [SerializeField] protected bool _doesInterrupt;
    [ReadOnly, SerializeField] private AttackResult attackResult;
    public AttackResult AttackResult
    {
        get => attackResult;
        protected set
        {
            attackResult = value;
            Context.SetAttackResult(value);
        }
    }
    public override void Enter()
    {
        AttackResult = AttackResult.None;
        base.Enter();
    }
    public override void Exit()
    {
        base.Exit();
    }
}