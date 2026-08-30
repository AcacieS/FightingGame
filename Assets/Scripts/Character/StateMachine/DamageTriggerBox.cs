using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A trigger collider that only deals damage while armed, so an attack has a real active
/// window instead of a single-frame snapshot.
///
/// Put this on a child object holding the collider (e.g. BiteColliderBox) and let the
/// attacker call Arm/Disarm around the contact frames. Damage lands once per arming, so
/// standing inside the box does not stack hits.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class DamageTriggerBox : MonoBehaviour
{
    [Tooltip("Who is swinging. Left empty, the nearest Character up the hierarchy is used.")]
    [SerializeField] private Character owner;

    [Tooltip("Layers this box can damage.")]
    [SerializeField] private LayerMask targetLayer = ~0;

    [Tooltip("Draws the box in the Scene view: grey when idle, red while armed.")]
    [SerializeField] private bool drawGizmos = true;

    private Collider2D box;
    private int damage;
    private bool interrupts;
    private bool armed;

    // Cleared on every Arm, so one swing can only hit a given character once.
    private readonly HashSet<Character> hitThisSwing = new();

    public bool IsArmed => armed;

    /// <summary>
    /// Raised with the victim each time this box lands a hit, so the attacker can react -
    /// impact audio, hitstop, a counter. Fires in step with hitThisSwing, so it is raised
    /// once per character per swing rather than every frame of the overlap.
    /// </summary>
    public event System.Action<Character> Landed;

    private void Awake()
    {
        box = GetComponent<Collider2D>();
        box.isTrigger = true;

        if (owner == null)
            owner = GetComponentInParent<Character>();

        if (owner == null)
            Debug.LogError($"{name}: no owning Character found, so this box cannot deal damage.", this);

        Disarm();
    }

    /// <summary>Opens the damage window. Call at the contact frame of the attack.</summary>
    public void Arm(int amount, bool isInterruptible)
    {
        damage = amount;
        interrupts = isInterruptible;

        hitThisSwing.Clear();
        armed = true;

        if (box != null)
            box.enabled = true;
    }

    /// <summary>Closes the damage window. Safe to call when already disarmed.</summary>
    public void Disarm()
    {
        armed = false;

        if (box != null)
            box.enabled = false;
    }

    // Enter alone is not enough: the collider is disabled between swings, so a player
    // already standing inside it when it is re-enabled may never generate an enter event.
    // Stay covers that case, and hitThisSwing keeps it to one hit either way.
    private void OnTriggerEnter2D(Collider2D other) => TryDamage(other);

    private void OnTriggerStay2D(Collider2D other) => TryDamage(other);

    private void TryDamage(Collider2D other)
    {
        if (!armed || owner == null)
            return;

        if ((targetLayer.value & (1 << other.gameObject.layer)) == 0)
            return;

        // InParent, not GetComponent: the collider is usually on a child of the character.
        Character victim = other.GetComponentInParent<Character>();

        if (victim == null || victim == owner)
            return;

        if (!hitThisSwing.Add(victim))
            return;

        owner.Hit(victim, damage, interrupts);

        Landed?.Invoke(victim);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos)
            return;

        Collider2D preview = box != null ? box : GetComponent<Collider2D>();

        if (preview == null)
            return;

        Gizmos.color = armed
            ? Color.red
            : new Color(1f, 1f, 1f, 0.25f);

        Bounds bounds = preview.bounds;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
    }
}
