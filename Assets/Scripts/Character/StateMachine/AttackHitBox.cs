using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Character owner;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private int damage = 10;
    [SerializeField] private float attackRange;
    [SerializeField] LayerMask charactersLayer;

    void Attack()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, charactersLayer);
        foreach(Collider2D enemy in hitEnemies)
        {
            Debug.Log("We hit" + enemy.name);
        }
    }
    void OnDrawGizmosSelected()
    {
        if(attackPoint==null) return;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}