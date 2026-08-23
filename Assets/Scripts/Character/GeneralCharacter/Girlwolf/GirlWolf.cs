using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GirlWolf boss "body".
///
/// This script performs moves, it never decides which move to make. Deciding stays
/// in the State / UtilityAction tree under the StateMachine GameObject, exactly like
/// the rest of the project. A State drives this through the public API below, e.g.
///
///     if (AI.Character is GirlWolf wolf)
///         wolf.Pounce();
///
/// Playstyle is agile pouncer: leap in from range, bite on the way down, dash back
/// out, re-approach.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class GirlWolf : Enemy
{
    public enum WolfMove
    {
        None,
        Bite,
        Pounce,
        DashBack
    }

    [Header("Movement")]
    [Tooltip("Used when the CharacterInfo asset leaves MoveSpeed at 0.")]
    [SerializeField] private float fallbackMoveSpeed = 4f;
    [SerializeField] private float acceleration = 25f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer = 1 << 3; // "Ground"

    [Header("Targeting")]
    [Tooltip("Layers an attack can damage. Player sits on Default, so leave this as Everything unless you add a Player layer.")]
    [SerializeField] private LayerMask targetLayer = ~0;

    [Header("Bite")]
    [SerializeField] private int biteDamage = 8;
    [SerializeField] private float biteRange = 1.8f;
    [SerializeField] private float biteWindup = 0.15f;
    [SerializeField] private float biteRecovery = 0.25f;
    [SerializeField] private float biteCooldown = 0.9f;
    [SerializeField] private Vector2 biteHitboxOffset = new Vector2(0.9f, 0f);
    [SerializeField] private Vector2 biteHitboxSize = new Vector2(1.4f, 1.2f);

    [Header("Pounce")]
    [SerializeField] private int pounceDamage = 14;
    [SerializeField] private float pounceMinRange = 2.5f;
    [SerializeField] private float pounceMaxRange = 7f;
    [SerializeField] private float pounceWindup = 0.35f;
    [SerializeField] private float pounceHorizontalSpeed = 9f;
    [SerializeField] private float pounceVerticalSpeed = 7f;
    [SerializeField] private float pounceRecovery = 0.5f;
    [SerializeField] private float pounceCooldown = 3f;
    [Tooltip("Safety net so a pounce that never lands cannot lock the boss up.")]
    [SerializeField] private float pounceMaxAirTime = 2.5f;
    [SerializeField] private Vector2 pounceHitboxOffset = new Vector2(0.7f, -0.2f);
    [SerializeField] private Vector2 pounceHitboxSize = new Vector2(1.6f, 1.6f);

    [Header("Dash Back")]
    [SerializeField] private float dashBackSpeed = 10f;
    [SerializeField] private float dashBackDuration = 0.2f;
    [SerializeField] private float dashBackRecovery = 0.15f;
    [SerializeField] private float dashBackCooldown = 1.5f;

    [Header("Animator Triggers")]
    [Tooltip("Triggers missing from the controller are skipped silently, so these are safe to leave as-is until the clips exist.")]
    [SerializeField] private string biteTrigger = "Bite";
    [SerializeField] private string pounceWindupTrigger = "PounceWindup";
    [SerializeField] private string pounceLaunchTrigger = "Pounce";
    [SerializeField] private string pounceLandTrigger = "PounceLand";
    [SerializeField] private string dashBackTrigger = "DashBack";

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [ReadOnly, SerializeField] private WolfMove currentMove = WolfMove.None;
    [ReadOnly, SerializeField] private bool grounded;
    [ReadOnly, SerializeField] private bool isDead;

    /// <summary>Raised when a move finishes its recovery and the boss is free again.</summary>
    public event System.Action<WolfMove> OnMoveFinished;

    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine moveRoutine;

    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitBuffer = new();
    private readonly HashSet<string> animatorParameters = new();

    private float moveDirection;
    private float lastMoveCommandTime = -Mathf.Infinity;

    private float lastBiteTime = -Mathf.Infinity;
    private float lastPounceTime = -Mathf.Infinity;
    private float lastDashBackTime = -Mathf.Infinity;

    // A State drives walking by calling MoveTowardsTarget() every Update. If it stops
    // calling (or forgets StopMoving() in Exit) the command goes stale and the boss
    // decelerates on its own instead of sliding away forever.
    private const float MoveCommandTimeout = 0.1f;

    #region Public API

    public bool IsBusy => currentMove != WolfMove.None;
    public bool IsDead => isDead;
    public bool IsGrounded => grounded;
    public WolfMove CurrentMove => currentMove;

    public float MoveSpeed =>
        Info != null && Info.MoveSpeed > 0f ? Info.MoveSpeed : fallbackMoveSpeed;

    public bool CanBite =>
        CanAct &&
        Time.time >= lastBiteTime + biteCooldown &&
        DistanceToTarget <= biteRange;

    public bool CanPounce =>
        CanAct &&
        grounded &&
        Time.time >= lastPounceTime + pounceCooldown &&
        DistanceToTarget >= pounceMinRange &&
        DistanceToTarget <= pounceMaxRange;

    public bool CanDashBack =>
        CanAct &&
        grounded &&
        Time.time >= lastDashBackTime + dashBackCooldown;

    /// <summary>Walk toward the target. Call every frame from ApproachState.Update().</summary>
    public void MoveTowardsTarget() => SetMoveCommand(DirectionToTarget);

    /// <summary>Walk away from the target. Call every frame from RetreatState.Update().</summary>
    public void MoveAwayFromTarget() => SetMoveCommand(-DirectionToTarget);

    public void StopMoving()
    {
        moveDirection = 0f;
        lastMoveCommandTime = -Mathf.Infinity;
    }

    /// <summary>
    /// Convenience entry point for the shared AttackState: pounces when there is room,
    /// otherwise bites. Boss-specific states should call Pounce()/Bite() directly.
    /// </summary>
    public bool Attack()
    {
        if (CanPounce)
            return Pounce();

        return Bite();
    }

    /// <summary>Returns false when busy, dead, or still on cooldown.</summary>
    public bool Bite()
    {
        if (!CanAct || Time.time < lastBiteTime + biteCooldown)
            return false;

        StartMove(BiteRoutine());
        return true;
    }

    public bool Pounce()
    {
        if (!CanAct || !grounded || Time.time < lastPounceTime + pounceCooldown)
            return false;

        StartMove(PounceRoutine());
        return true;
    }

    public bool DashBack()
    {
        if (!CanAct || Time.time < lastDashBackTime + dashBackCooldown)
            return false;

        StartMove(DashBackRoutine());
        return true;
    }

    /// <summary>Interrupts whatever move is running, e.g. when the boss is staggered.</summary>
    public void CancelMove()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }

        currentMove = WolfMove.None;
        StopMoving();
        StopHorizontal();
    }

    #endregion

    #region Unity

    public override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // RequireComponent adds a default Rigidbody2D; without this the boss topples
        // over the first time it is pushed.
        rb.freezeRotation = true;

        hitFilter = new ContactFilter2D { useTriggers = true };
        hitFilter.SetLayerMask(targetLayer);

        CacheAnimatorParameters();

        if (targetLayer == 0)
            Debug.LogWarning($"{name}: Target Layer is set to Nothing, so attacks will never connect.", this);
    }

    public override void Start()
    {
        base.Start();

        if (Context.Instance == null)
            Debug.LogWarning($"{name}: no Context in the scene, so distance-based moves will never trigger.", this);
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        ).collider != null;

        ApplyWalk();
    }

    public override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        CancelMove();

        // Hp is assigned during base.Awake(), so Die() can fire before rb is cached.
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        base.Die();
    }

    #endregion

    #region Moves

    private IEnumerator BiteRoutine()
    {
        currentMove = WolfMove.Bite;
        lastBiteTime = Time.time;

        StopMoving();
        StopHorizontal();
        PlayTrigger(biteTrigger);

        yield return new WaitForSeconds(biteWindup);

        TryHit(biteHitboxOffset, biteHitboxSize, biteDamage);

        yield return new WaitForSeconds(biteRecovery);

        FinishMove();
    }

    private IEnumerator PounceRoutine()
    {
        currentMove = WolfMove.Pounce;
        lastPounceTime = Time.time;

        // Plant the feet and telegraph, so the player has a window to react.
        StopMoving();
        StopHorizontal();
        PlayTrigger(pounceWindupTrigger);

        yield return new WaitForSeconds(pounceWindup);

        float direction = DirectionToTarget;
        rb.linearVelocity = new Vector2(
            direction * pounceHorizontalSpeed,
            pounceVerticalSpeed
        );
        PlayTrigger(pounceLaunchTrigger);

        // One frame is not enough to clear the ground raycast, so wait until we are
        // genuinely airborne before letting the landing check take over.
        yield return new WaitForSeconds(0.1f);

        float airTime = 0f;
        bool hasHit = false;

        while (airTime < pounceMaxAirTime && !HasLanded())
        {
            // One target per leap, otherwise the hitbox chews through the player.
            if (!hasHit)
                hasHit = TryHit(pounceHitboxOffset, pounceHitboxSize, pounceDamage);

            airTime += Time.deltaTime;
            yield return null;
        }

        StopHorizontal();
        PlayTrigger(pounceLandTrigger);

        yield return new WaitForSeconds(pounceRecovery);

        FinishMove();
    }

    private IEnumerator DashBackRoutine()
    {
        currentMove = WolfMove.DashBack;
        lastDashBackTime = Time.time;

        StopMoving();
        PlayTrigger(dashBackTrigger);

        float direction = -DirectionToTarget;
        float elapsed = 0f;

        while (elapsed < dashBackDuration)
        {
            rb.linearVelocity = new Vector2(
                direction * dashBackSpeed,
                rb.linearVelocity.y
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        StopHorizontal();

        yield return new WaitForSeconds(dashBackRecovery);

        FinishMove();
    }

    private void StartMove(IEnumerator routine)
    {
        CancelMove();
        moveRoutine = StartCoroutine(routine);
    }

    private void FinishMove()
    {
        WolfMove finished = currentMove;

        currentMove = WolfMove.None;
        moveRoutine = null;

        OnMoveFinished?.Invoke(finished);
    }

    #endregion

    #region Helpers

    private bool CanAct => !isDead && !IsBusy;

    private Character TargetCharacter =>
        Context.Instance != null ? Context.Instance.Target : null;

    private float DistanceToTarget
    {
        get
        {
            Character target = TargetCharacter;

            if (target == null)
                return Mathf.Infinity;

            return Vector2.Distance(transform.position, target.transform.position);
        }
    }

    /// <summary>+1 when facing/heading right. Falls back to current facing when there is no target.</summary>
    private float DirectionToTarget
    {
        get
        {
            Character target = TargetCharacter;

            if (target != null)
            {
                float delta = target.transform.position.x - transform.position.x;

                if (!Mathf.Approximately(delta, 0f))
                    return Mathf.Sign(delta);
            }

            return Facing;
        }
    }

    // Character.LookAt flips localScale.x toward the target, so the sign of the scale
    // is the facing and the hitbox offsets mirror with it.
    private float Facing => transform.localScale.x < 0f ? -1f : 1f;

    private void SetMoveCommand(float direction)
    {
        if (!CanAct)
            return;

        moveDirection = direction;
        lastMoveCommandTime = Time.time;
    }

    private void ApplyWalk()
    {
        if (IsBusy || isDead)
            return;

        if (Time.time - lastMoveCommandTime > MoveCommandTimeout)
            moveDirection = 0f;

        float targetSpeed = moveDirection * MoveSpeed;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    private void StopHorizontal()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private bool HasLanded() => grounded && rb.linearVelocity.y <= 0.01f;

    private bool TryHit(Vector2 offset, Vector2 size, int damage)
    {
        Vector2 center = (Vector2)transform.position +
                         new Vector2(offset.x * Facing, offset.y);

        int count = Physics2D.OverlapBox(center, size, 0f, hitFilter, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            Character victim = hitBuffer[i].GetComponentInParent<Character>();

            if (victim == null || victim == this)
                continue;

            Hit(victim, damage);
            return true;
        }

        return false;
    }

    private void CacheAnimatorParameters()
    {
        animatorParameters.Clear();

        if (animator.runtimeAnimatorController == null)
            return;

        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            animatorParameters.Add(parameter.name);
        }
    }

    // Skips triggers the controller does not declare, so the boss runs without warnings
    // while the animations are still being authored.
    private void PlayTrigger(string trigger)
    {
        if (string.IsNullOrEmpty(trigger) || !animatorParameters.Contains(trigger))
            return;

        animator.SetTrigger(trigger);
    }

    #endregion

    private void OnDrawGizmosSelected()
    {
        if (!drawGizmos)
            return;

        float facing = Facing;
        Vector2 position = transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(
            position + new Vector2(biteHitboxOffset.x * facing, biteHitboxOffset.y),
            biteHitboxSize
        );

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireCube(
            position + new Vector2(pounceHitboxOffset.x * facing, pounceHitboxOffset.y),
            pounceHitboxSize
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, pounceMinRange);
        Gizmos.DrawWireSphere(position, pounceMaxRange);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(position, position + Vector2.down * groundCheckDistance);
    }
}
