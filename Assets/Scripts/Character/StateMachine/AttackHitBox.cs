using UnityEngine;

public class AttackHitbox : MonoBehaviour
{
    [SerializeField] private Character owner;
    [SerializeField] private int damage = 10;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Character target = other.GetComponent<Character>();

        if (target == null || target == owner)
            return;

        target.Hurt(damage);
    }
}