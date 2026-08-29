using UnityEngine;

public class NearAttackState : AttackState
{
    [Header("Near Attack State")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask charactersLayer;
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Near Attack");
    }

    public override void Play()
    {
        if(animName==" ")
        {
            RequestDecision();
            Debug.LogError("animName not Assigned");
            return;
        }
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestDecision();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("AI → Exit Near Attack");
    }
    public bool Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, charactersLayer);
        foreach(Collider2D enemy in hitEnemies)
        {
            Character character = enemy.GetComponent<Character>();
            //TODO: It just search for the same gameObject Character.
            if( character == Context.Self) continue;
            OnAttackHit();
            return true;
        }
        return false;
    }
    protected virtual void OnAttackHit()
    {
        bool isHurt = Context.Target.Hurt(damage, _doesInterrupt, _stunDuration);
        //TODO: CHECK SUCCESSFULLY
        attackResult = isHurt? AttackResult.Success: AttackResult.Blocked;
        
    }
    void OnDrawGizmosSelected()
    {
        if(attackPoint==null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}