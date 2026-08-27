using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Character owner;
    [SerializeField] private Character target;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask charactersLayer;
    [ReadOnly, SerializeField] private AttackResult attackResult;
    public AttackResult AttackResult => attackResult;
    private void Awake()
    {
        if (owner == null)
        {
            owner = Context.Instance.Self;
            target = Context.Instance.Target;
        }
    }
    public bool Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, charactersLayer);
        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
            Character character = enemy.GetComponent<Character>();
            //TODO: It just search for the same gameObject Character.
            if( character == owner) continue;
            OnAttackHit();
            return true;
        }
        return false;
    }
    protected virtual void OnAttackHit()
    {
        bool isHurt = target.Hurt(damage);
        //TODO: CHECK SUCCESSFULLY
        attackResult = isHurt? AttackResult.Success: AttackResult.Blocked;
        
    }
    
    void OnDrawGizmosSelected()
    {
        if(attackPoint==null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}