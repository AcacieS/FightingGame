using UnityEngine;

public class NearAttackState : State
{
    [Header("Attack State")]
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask charactersLayer;
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Near Attack");
        Attack();
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
    private void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, charactersLayer);
        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
            if(enemy.GetComponent<Character>() == Context.Self) continue;
            Context.Target.Hurt(damage);
            return;
        }
    }
    void OnDrawGizmosSelected()
    {
        if(attackPoint==null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}