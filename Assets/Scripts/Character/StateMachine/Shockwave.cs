using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A travelling hitbox thrown out by an attack. Flies in a straight horizontal line at a
/// fixed speed; the direction is locked in at Launch and never re-aimed, so it commits to
/// where the target was when it spawned and sidestepping afterwards actually works.
///
/// Put this on a prefab with a trigger Collider2D, then have the attacker Instantiate it
/// and call Launch(). Nothing here knows about GirlWolf - any Character can fire one.
///
/// The fields below describe the projectile itself and live on the prefab. Per-shot values
/// (speed, damage, direction) are passed in by whoever fires it.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public class Shockwave : MonoBehaviour
{
    [Header("Lifetime")]
    [Tooltip("Seconds before it despawns on its own, so a wave that misses does not live forever. 0 = never expires.")]
    [SerializeField] private float lifetime = 3f;
    [Tooltip("Despawns once it has travelled this far from where it spawned. 0 = no limit.")]
    [SerializeField] private float maxDistance;

    [Header("Damage")]
    [Tooltip("Layers this can damage. The character that fired it is always ignored regardless.")]
    [SerializeField] private LayerMask targetLayer = ~0;
    [Tooltip("On: dies on its first hit. Off: passes through and can still hit someone else.")]
    [SerializeField] private bool destroyOnHit = true;

    [Header("Presentation")]
    [Tooltip("Mirrors the sprite so the wave always leans the way it is travelling.")]
    [SerializeField] private bool faceTravelDirection = true;

    private Rigidbody2D body;
    private Character owner;
    private Vector2 spawnPoint;
    private float direction = 1f;
    private float speed;
    private int damage;
    private bool interrupts;
    private bool launched;

    // One hit per character, so a wave that lingers on top of someone does not tick them
    // down frame by frame through OnTriggerStay2D.
    private readonly HashSet<Character> alreadyHit = new();

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();

        // Kinematic: the wave must not fall, be pushed, or slow down. It needs a body at
        // all because a collider that moves without one is treated as static geometry,
        // which makes its trigger events unreliable.
        body.bodyType = RigidbodyType2D.Kinematic;
        body.gravityScale = 0f;

        Collider2D box = GetComponent<Collider2D>();

        if (box == null)
        {
            Debug.LogError($"{name}: no Collider2D, so this shockwave can never hit anything.", this);
            enabled = false;
            return;
        }

        box.isTrigger = true;
    }

    /// <summary>
    /// Sends the wave on its way. <paramref name="travelDirection"/> only needs a sign;
    /// it is normalised to +/-1 here, and never re-read afterwards.
    /// </summary>
    public void Launch(Character source, float travelDirection, float travelSpeed, int hitDamage, bool isInterruptible)
    {
        owner = source;
        direction = travelDirection < 0f ? -1f : 1f;
        speed = travelSpeed;
        damage = hitDamage;
        interrupts = isInterruptible;

        spawnPoint = transform.position;
        launched = true;

        if (faceTravelDirection)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * direction;
            transform.localScale = scale;
        }

        body.linearVelocity = new Vector2(direction * speed, 0f);

        if (lifetime > 0f)
            Destroy(gameObject, lifetime);
    }

    private void FixedUpdate()
    {
        if (!launched)
            return;

        // Re-asserted every step rather than set once at launch: anything that nudges the
        // body would otherwise alter its course permanently.
        body.linearVelocity = new Vector2(direction * speed, 0f);

        if (maxDistance > 0f &&
            Mathf.Abs(transform.position.x - spawnPoint.x) >= maxDistance)
        {
            Destroy(gameObject);
        }
    }

    // Stay as well as Enter, matching DamageTriggerBox: a target that is already overlapping
    // the spawn point may never generate an enter event. alreadyHit keeps it to one hit.
    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    private void TryDamage(Collider2D other)
    {
        // Unity's overloaded == also reports a destroyed owner as null, so a wave outlives
        // its caster harmlessly instead of throwing.
        if (!launched || owner == null)
            return;

        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        // InParent, not GetComponent: the collider is usually on a child of the character.
        Character victim = other.GetComponentInParent<Character>();

        if (victim == null || victim == owner)
            return;

        if (!alreadyHit.Add(victim))
            return;

        owner.Hit(victim, damage, interrupts);

        if (destroyOnHit)
            Destroy(gameObject);
    }
}
