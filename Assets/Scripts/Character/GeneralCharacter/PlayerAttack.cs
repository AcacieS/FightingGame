using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

// Red Riding Hood combat. Lives next to Player on the same GameObject:
// PlayerInput (SendMessages) delivers OnAttack/OnMove to every component here.
// Damage is applied with an instant overlap box so no animation events are
// needed yet; swap to animation-driven hit frames once the sprites arrive.
public class PlayerAttack : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public string animName;
        public int damage = 10;
        [Tooltip("Hitbox center relative to the player, x flips with facing")]
        public Vector2 offset = new Vector2(0.9f, 0f);
        public Vector2 size = new Vector2(1.2f, 1f);
    }

    [SerializeField] private Character character;
    [SerializeField] private LayerMask hitMask = ~0;
    [SerializeField] private float cooldown = 0.35f;

    [Header("Attacks (pick by held direction)")]
    [SerializeField] private AttackData sweep = new AttackData
    { animName = "Attack", damage = 10, offset = new Vector2(0.9f, 0f), size = new Vector2(1.2f, 1f) };
    [SerializeField] private AttackData uppercut = new AttackData
    { animName = "Uppercut", damage = 12, offset = new Vector2(0.5f, 1f), size = new Vector2(1f, 1.4f) };
    [SerializeField] private AttackData downStrike = new AttackData
    { animName = "DownStrike", damage = 12, offset = new Vector2(0.5f, -0.8f), size = new Vector2(1f, 1f) };

    private Vector2 moveInput;
    private float lastAttackTime = -999f;

    private AttackData lastAttack;

    private void Awake()
    {
        if (character == null)
        {
            character = GetComponent<Character>();
        }
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        Player player = character as Player;
        if (Mathf.Abs(moveInput.x) > 0.01f
            && (player == null || (!player.IsStunned && !player.IsControlsLocked)))
        {
            Face(Mathf.Sign(moveInput.x));
        }
    }

    public void OnAttack(InputValue value)
    {
        Player player = character as Player;
        if(player != null && (player.IsStunned || player.IsControlsLocked || player.IsBlocking))
            return;
        
        if (!value.isPressed || Time.time < lastAttackTime + cooldown)
            return;

        lastAttackTime = Time.time;

        AttackData attack = sweep;
        if (moveInput.y > 0.5f)
            attack = uppercut;
        else if (moveInput.y < -0.5f)
            attack = downStrike;

        DoAttack(attack);
    }

    private void DoAttack(AttackData attack)
    {
        lastAttack = attack;
        character.PlayAnim(attack.animName);

        float facing = Mathf.Sign(transform.localScale.x);
        Vector2 center = (Vector2)transform.position
            + new Vector2(attack.offset.x * facing, attack.offset.y);

        Collider2D[] hits = Physics2D.OverlapBoxAll(center, attack.size, 0f, hitMask);
        HashSet<Character> alreadyHit = new HashSet<Character>();

        foreach (Collider2D col in hits)
        {
            Character target = col.GetComponentInParent<Character>();
            if (target == null || target == character || !alreadyHit.Add(target))
                continue;

            character.Hit(target, attack.damage);
            Debug.Log($"{name} hit {target.name} for {attack.damage} -> HP now {target.Hp}");
        }
    }

    private void Face(float sign)
    {
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * sign;
        transform.localScale = scale;
    }

    private void OnDrawGizmos()
    {
        float facing = Mathf.Sign(transform.localScale.x);
        foreach (AttackData a in new[] { sweep, uppercut, downStrike })
        {
            if (a == null)
                continue;
            
            bool justFired = a == lastAttack && Time.time < lastAttackTime +0.25f;
            Gizmos.color = justFired ? Color.red : new Color(1f, 1f, 1f, 0.25f);
            Vector2 center = (Vector2)transform.position
                + new Vector2(a.offset.x * facing, a.offset.y);
            Gizmos.DrawWireCube(center, a.size);
        }
    }
}
