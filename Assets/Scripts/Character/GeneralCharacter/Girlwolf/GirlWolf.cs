using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GirlWolf boss.
///
/// Wandering is the hub state. The boss prowls back and forth there until something
/// triggers a move, performs that move, then drops straight back into Wandering.
///
///     Wandering --hp below 30%----> Accumulate --+
///               --target within 1.5-> Bite     --+
///               --Pounce() called---> Pounce   --+--> back to Wandering
///               --Dash() called-----> Dash     --+
///
/// Bite and Accumulate trigger themselves off the world. Pounce and Dash keep the
/// gates they always had and are still driven from the State / UtilityAction tree:
///
///     if (AI.Character is GirlWolf wolf)
///         wolf.Pounce();
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Animator))]
public class GirlWolf : Enemy
{
    // Wandering is first so the zero value is the resting state - a serialized field
    // left at default then reads as Wandering rather than as a half-started attack.
    public enum WolfMove
    {
        Wandering,
        Bite,
        Dash,
        Pounce,
        Accumulate
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
    [Tooltip("Tag the boss hunts for. The player prefab must actually carry this tag.")]
    [SerializeField] private string targetTag = "Player";
    [Tooltip("Seconds between re-scans while no target is held. Match spawns the player at runtime, so the boss has to keep looking.")]
    [SerializeField] private float targetSearchInterval = 0.5f;
    [Tooltip("Let the boss turn to face its target. Character.LookAt also does this whenever a Context is configured.")]
    [SerializeField] private bool controlFacing = true;

    [Header("Wandering (idle prowl)")]
    [Tooltip("Fraction of MoveSpeed used while prowling, so idling reads slower than a committed approach.")]
    [SerializeField, Range(0f, 1f)] private float wanderingMoveScale = 0.5f;
    [SerializeField] private float wanderingMinWalkTime = 0.4f;
    [SerializeField] private float wanderingMaxWalkTime = 1.2f;
    [SerializeField] private float wanderingMinPauseTime = 0.3f;
    [SerializeField] private float wanderingMaxPauseTime = 1f;

    [Header("Bite")]
    [SerializeField] private int biteDamage = 8;
    [Tooltip("Wandering auto-triggers a bite when the target is closer than this.")]
    [SerializeField] private float biteRange = 1.5f;
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

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashRecovery = 0.15f;
    [SerializeField] private float dashCooldown = 1.5f;

    [Header("Accumulate (desperation attack)")]
    [Tooltip("Wandering auto-triggers Accumulate once HP drops below this fraction of max.")]
    [SerializeField, Range(0f, 1f)] private float accumulateHealthThreshold = 0.3f;
    [SerializeField] private float accumulateChargeTime = 3f;
    [Tooltip("Delay between the release trigger and the hit. Move this onto an animation event once the release clip exists.")]
    [SerializeField] private float accumulateReleaseWindup = 0.2f;
    [SerializeField] private float accumulateRecovery = 0.8f;
    [Tooltip("Stops the boss chain-casting once it is permanently below the HP threshold. Set very high for once per fight.")]
    [SerializeField] private float accumulateCooldown = 8f;
    [SerializeField] private int accumulateDamage = 30;
    [SerializeField] private Vector2 accumulateHitboxOffset = new Vector2(1.2f, 0f);
    [SerializeField] private Vector2 accumulateHitboxSize = new Vector2(3.5f, 2.5f);

    [Header("Animator Triggers")]
    [Tooltip("Triggers missing from the controller are skipped silently, so these are safe to leave as-is until the clips exist.")]
    [SerializeField] private string wanderingTrigger = "Wandering";
    [SerializeField] private string biteTrigger = "Bite";
    [SerializeField] private string pounceWindupTrigger = "PounceWindup";
    [SerializeField] private string pounceLaunchTrigger = "Pounce";
    [SerializeField] private string pounceLandTrigger = "PounceLand";
    [SerializeField] private string dashTrigger = "Dash";
    [SerializeField] private string accumulateChargeTrigger = "Accumulate";
    [SerializeField] private string accumulateReleaseTrigger = "AccumulateRelease";

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [ReadOnly, SerializeField] private WolfMove currentMove = WolfMove.Wandering;
    [ReadOnly, SerializeField] private bool grounded;
    [ReadOnly, SerializeField] private bool isDead;

    /// <summary>Raised with the move that just ended, right before the boss returns to Wandering.</summary>
    public event System.Action<WolfMove> OnMoveFinished;

    /// <summary>Raised whenever the current move changes, including back into Wandering.</summary>
    public event System.Action<WolfMove> OnMoveChanged;

    /// <summary>Raised when the tracked target changes, including to null when it despawns.</summary>
    public event System.Action<Character> OnTargetChanged;

    private Rigidbody2D rb;
    private Animator animator;
    private Coroutine brainRoutine;

    private Character target;
    private float nextTargetSearchTime;
    private bool warnedNoTarget;

    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitBuffer = new();
    private readonly HashSet<string> animatorParameters = new();

    // The move Wandering should break out and run. Wandering means "nothing queued".
    private WolfMove pendingMove = WolfMove.Wandering;

    private float moveThrottle;
    private float lastMoveCommandTime = -Mathf.Infinity;
    private float lastExternalMoveTime = -Mathf.Infinity;

    private float lastBiteTime = -Mathf.Infinity;
    private float lastPounceTime = -Mathf.Infinity;
    private float lastDashTime = -Mathf.Infinity;
    private float lastAccumulateTime = -Mathf.Infinity;

    // A State drives walking by calling MoveTowardsTarget() every Update. If it stops
    // calling (or forgets StopMoving() in Exit) the command goes stale and the boss
    // decelerates on its own instead of sliding away forever.
    private const float MoveCommandTimeout = 0.1f;

    #region Public API

    public bool IsBusy => currentMove != WolfMove.Wandering;
    public bool IsDead => isDead;
    public bool IsGrounded => grounded;
    public WolfMove CurrentMove => currentMove;

    /// <summary>The tracked player, or null when none is in the scene. Does not trigger a search.</summary>
    public Character Target => target;

    /// <summary>True only while a target exists and still has HP. Attacks are gated on this.</summary>
    public bool TargetIsAlive => target != null && target.Hp > 0;

    public float MoveSpeed =>
        Info != null && Info.MoveSpeed > 0f ? Info.MoveSpeed : fallbackMoveSpeed;

    // Falls back to full, not empty: an unassigned CharacterInfo would otherwise read as
    // 0% health and pin the boss into Accumulate forever.
    public float HealthFraction =>
        Info != null && Info.Hp > 0 ? (float)Hp / Info.Hp : 1f;

    public bool CanBite =>
        CanAct &&
        Time.time >= lastBiteTime + biteCooldown &&
        DistanceToTarget < biteRange;

    public bool CanPounce =>
        CanAct &&
        grounded &&
        Time.time >= lastPounceTime + pounceCooldown &&
        DistanceToTarget >= pounceMinRange &&
        DistanceToTarget <= pounceMaxRange;

    public bool CanDash =>
        CanAct &&
        grounded &&
        Time.time >= lastDashTime + dashCooldown;

    public bool CanAccumulate =>
        CanAct &&
        HealthFraction < accumulateHealthThreshold &&
        Time.time >= lastAccumulateTime + accumulateCooldown;

    /// <summary>Walk toward the target. Call every frame from ApproachState.Update().</summary>
    public void MoveTowardsTarget() => SetExternalMoveCommand(DirectionToTarget);

    /// <summary>Walk away from the target. Call every frame from RetreatState.Update().</summary>
    public void MoveAwayFromTarget() => SetExternalMoveCommand(-DirectionToTarget);

    public void StopMoving()
    {
        moveThrottle = 0f;
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

    /// <summary>Queues a move. Returns false when busy, dead, or still on cooldown.</summary>
    public bool Bite() => TryQueue(WolfMove.Bite, Time.time >= lastBiteTime + biteCooldown);

    public bool Pounce() => TryQueue(WolfMove.Pounce, grounded && Time.time >= lastPounceTime + pounceCooldown);

    public bool Dash() => TryQueue(WolfMove.Dash, grounded && Time.time >= lastDashTime + dashCooldown);

    public bool Accumulate() => TryQueue(WolfMove.Accumulate, Time.time >= lastAccumulateTime + accumulateCooldown);

    /// <summary>Interrupts whatever is running and drops the boss back into Wandering.</summary>
    public void CancelMove()
    {
        if (brainRoutine != null)
        {
            StopCoroutine(brainRoutine);
            brainRoutine = null;
        }

        SetMove(WolfMove.Wandering);
        pendingMove = WolfMove.Wandering;

        StopMoving();
        StopHorizontal();

        if (!isDead && isActiveAndEnabled)
            brainRoutine = StartCoroutine(BrainRoutine());
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
        // Started before base.Start() so a throw in the base class can never leave the
        // boss alive but brainless. Character logs its own missing-Context warning.
        brainRoutine = StartCoroutine(BrainRoutine());

        base.Start();
    }

    private void FixedUpdate()
    {
        grounded = Physics2D.Raycast(
            transform.position,
            Vector2.down,
            groundCheckDistance,
            groundLayer
        ).collider != null;

        UpdateFacing();
        ApplyWalk();
    }

    public override void Die()
    {
        if (isDead)
            return;

        isDead = true;

        // isDead is set first so CancelMove does not restart the brain.
        CancelMove();

        // Hp is assigned during base.Awake(), so Die() can fire before rb is cached.
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        base.Die();
    }

    #endregion

    #region Brain

    // One long-lived coroutine owns the whole cycle. Moves are nested with
    // "yield return Routine()" rather than StartCoroutine, so stopping the brain in
    // CancelMove tears down the running move with it.
    private IEnumerator BrainRoutine()
    {
        while (!isDead)
        {
            yield return WanderingRoutine();

            if (isDead)
                yield break;

            WolfMove move = pendingMove;
            pendingMove = WolfMove.Wandering;

            switch (move)
            {
                case WolfMove.Bite:
                    yield return BiteRoutine();
                    break;

                case WolfMove.Pounce:
                    yield return PounceRoutine();
                    break;

                case WolfMove.Dash:
                    yield return DashRoutine();
                    break;

                case WolfMove.Accumulate:
                    yield return AccumulateRoutine();
                    break;

                default:
                    continue;
            }

            OnMoveFinished?.Invoke(move);
        }
    }

    /// <summary>
    /// The resting state: prowl left/pause/right/pause at random until a trigger fires
    /// or something external queues a move.
    /// </summary>
    private IEnumerator WanderingRoutine()
    {
        SetMove(WolfMove.Wandering);
        StopMoving();
        PlayTrigger(wanderingTrigger);

        float stepTimer = 0f;
        float wanderDirection = 0f;
        bool pausing = true;

        while (pendingMove == WolfMove.Wandering && !isDead)
        {
            if (CheckWanderingTriggers())
                break;

            // A State is steering us this frame, so do not fight it with the prowl.
            if (Time.time - lastExternalMoveTime <= MoveCommandTimeout)
            {
                yield return null;
                continue;
            }

            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                pausing = !pausing;

                if (pausing)
                {
                    wanderDirection = 0f;
                    stepTimer = Random.Range(wanderingMinPauseTime, wanderingMaxPauseTime);
                }
                else
                {
                    wanderDirection = Random.value < 0.5f ? -1f : 1f;
                    stepTimer = Random.Range(wanderingMinWalkTime, wanderingMaxWalkTime);
                }
            }

            if (Mathf.Approximately(wanderDirection, 0f))
                StopMoving();
            else
                SetMoveCommand(wanderDirection * wanderingMoveScale);

            yield return null;
        }

        StopMoving();
    }

    /// <summary>Self-triggers that only fire out of Wandering. Accumulate outranks Bite.</summary>
    private bool CheckWanderingTriggers()
    {
        // Nothing to attack: keep prowling rather than mauling a corpse or empty air.
        if (!TargetIsAlive)
        {
            AcquireTarget();
            return false;
        }

        if (HealthFraction < accumulateHealthThreshold &&
            Time.time >= lastAccumulateTime + accumulateCooldown)
        {
            pendingMove = WolfMove.Accumulate;
            return true;
        }

        if (Time.time >= lastBiteTime + biteCooldown &&
            DistanceToTarget < biteRange)
        {
            pendingMove = WolfMove.Bite;
            return true;
        }

        return false;
    }

    // Single funnel for state changes so observers (and the test view) see every one.
    private void SetMove(WolfMove move)
    {
        if (currentMove == move)
            return;

        currentMove = move;
        OnMoveChanged?.Invoke(move);
    }

    private bool TryQueue(WolfMove move, bool offCooldown)
    {
        if (!CanAct || !offCooldown)
            return false;

        pendingMove = move;
        return true;
    }

    #endregion

    #region Moves

    private IEnumerator BiteRoutine()
    {
        SetMove(WolfMove.Bite);
        lastBiteTime = Time.time;

        StopMoving();
        StopHorizontal();
        PlayTrigger(biteTrigger);

        yield return new WaitForSeconds(biteWindup);

        TryHit(biteHitboxOffset, biteHitboxSize, biteDamage);

        yield return new WaitForSeconds(biteRecovery);
    }

    private IEnumerator PounceRoutine()
    {
        SetMove(WolfMove.Pounce);
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
    }

    private IEnumerator DashRoutine()
    {
        SetMove(WolfMove.Dash);
        lastDashTime = Time.time;

        StopMoving();
        PlayTrigger(dashTrigger);

        float direction = -DirectionToTarget;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.linearVelocity = new Vector2(
                direction * dashSpeed,
                rb.linearVelocity.y
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        StopHorizontal();

        yield return new WaitForSeconds(dashRecovery);
    }

    /// <summary>
    /// Desperation move: root in place, charge for accumulateChargeTime, then release
    /// one heavy hit. The release is timed in code for now; once the clip exists, drive
    /// the hit from an animation event and drop accumulateReleaseWindup to 0.
    /// </summary>
    private IEnumerator AccumulateRoutine()
    {
        SetMove(WolfMove.Accumulate);
        lastAccumulateTime = Time.time;

        StopMoving();
        StopHorizontal();
        PlayTrigger(accumulateChargeTrigger);

        yield return new WaitForSeconds(accumulateChargeTime);

        PlayTrigger(accumulateReleaseTrigger);

        yield return new WaitForSeconds(accumulateReleaseWindup);

        TryHit(accumulateHitboxOffset, accumulateHitboxSize, accumulateDamage);

        yield return new WaitForSeconds(accumulateRecovery);
    }

    #endregion

    #region Helpers

    private bool CanAct =>
        !isDead &&
        currentMove == WolfMove.Wandering &&
        pendingMove == WolfMove.Wandering;

    /// <summary>
    /// Finds and caches the player by tag. Match spawns the player at runtime and destroys
    /// it between matches, so this re-scans (throttled) whenever the cache has emptied.
    /// </summary>
    private Character TargetCharacter
    {
        get
        {
            // Unity's overloaded == reports a destroyed object as null, so this also
            // catches the player being destroyed at the end of a match.
            if (target != null)
                return target;

            AcquireTarget();
            return target;
        }
    }

    private void AcquireTarget()
    {
        if (Time.time < nextTargetSearchTime)
            return;

        nextTargetSearchTime = Time.time + targetSearchInterval;

        Character found = FindByTag();

        // Fall back to the Context wiring so an untagged player still works.
        if (found == null && Context.Instance != null)
            found = Context.Instance.Target;

        if (found == null)
        {
            if (!warnedNoTarget)
            {
                warnedNoTarget = true;
                Debug.LogWarning(
                    $"{name}: no Character tagged '{targetTag}' in the scene and no Context target. " +
                    "Set the tag on the player prefab.",
                    this
                );
            }
        }
        else
        {
            warnedNoTarget = false;
        }

        SetTarget(found);
    }

    private Character FindByTag()
    {
        if (string.IsNullOrEmpty(targetTag))
            return null;

        GameObject tagged;

        try
        {
            tagged = GameObject.FindGameObjectWithTag(targetTag);
        }
        catch (UnityException)
        {
            // An undefined tag throws every scan, which would kill the brain coroutine.
            // Blank it out so we stop retrying and fall through to the Context target.
            Debug.LogError($"{name}: tag '{targetTag}' is not defined in this project.", this);
            targetTag = string.Empty;
            return null;
        }

        if (tagged == null)
            return null;

        Character character = tagged.GetComponentInParent<Character>();

        if (character == null)
            character = tagged.GetComponentInChildren<Character>();

        return character;
    }

    private void SetTarget(Character next)
    {
        if (target == next)
            return;

        target = next;
        OnTargetChanged?.Invoke(next);
    }

    private void UpdateFacing()
    {
        // Only while idle: re-facing mid-attack would teleport the hitbox to the other side.
        if (!controlFacing || IsBusy || isDead)
            return;

        Character current = TargetCharacter;

        if (current == null)
            return;

        float delta = current.transform.position.x - transform.position.x;

        if (Mathf.Abs(delta) < 0.01f)
            return;

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(delta);
        transform.localScale = scale;
    }

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

    /// <summary>+1 when the target is to the right. Falls back to current facing when there is no target.</summary>
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

    private void SetExternalMoveCommand(float throttle)
    {
        lastExternalMoveTime = Time.time;
        SetMoveCommand(throttle);
    }

    // throttle is signed and normally +/-1, but Wandering scales it down to prowl.
    private void SetMoveCommand(float throttle)
    {
        if (isDead || IsBusy)
            return;

        moveThrottle = throttle;
        lastMoveCommandTime = Time.time;
    }

    private void ApplyWalk()
    {
        if (IsBusy || isDead)
            return;

        if (Time.time - lastMoveCommandTime > MoveCommandTimeout)
            moveThrottle = 0f;

        float targetSpeed = moveThrottle * MoveSpeed;

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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            position + new Vector2(accumulateHitboxOffset.x * facing, accumulateHitboxOffset.y),
            accumulateHitboxSize
        );

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, biteRange);
        Gizmos.DrawWireSphere(position, pounceMinRange);
        Gizmos.DrawWireSphere(position, pounceMaxRange);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(position, position + Vector2.down * groundCheckDistance);
    }
}
