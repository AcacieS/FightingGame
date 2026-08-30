using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// The GirlWolf boss.
///
/// Wandering is the hub. The boss stands there on the idle animation deciding what to
/// do, runs one move, then drops straight back into Wandering.
///
/// Priority order, top to bottom - the first match wins:
///
///     Wandering --hp below 30%-------------> Accumulate --+
///               --Scratch ready------------> Scratch    --+
///               --Bite ready---------------> Bite       --+--> back to Wandering
///               --Pounce ready-------------> Pounce     --+
///               --nothing ready, think-----> Chasing    --+
///
/// "Ready" means in range AND off cooldown - never range alone. The Chasing row is the
/// unconditional catch-all: if no attack is available the boss closes distance rather than
/// standing still, which is what stops it parking in Idle beside the player waiting for a
/// long cooldown to tick down.
///
/// Scratch is checked before Bite because it is the closer-range move: inside its range it
/// wins, and once it goes on cooldown the check falls through to the bite. Chasing walks at
/// the player and ends the moment an attack becomes available, handing back to Wandering
/// which then fires it; it also re-checks for Scratch on a fixed interval so a player who
/// closes the gap fast gets swiped mid-chase. Bite chains in place while the player stays
/// in range; Scratch is always a single swing.
///
/// Moves can also be driven from the State / UtilityAction tree:
///
///     if (AI.Character is GirlWolf wolf)
///         wolf.Pounce();
///
/// Movement and facing go through Character. Animation is parameter-driven: every state
/// owns one Bool on the Animator, raised on entry and lowered on exit by SetMove, so
/// exactly one is true at a time and the transition graph owns which clip that plays.
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
        Chasing,
        Bite,
        Scratch,
        Pounce,
        Accumulate
    }

    [Header("Movement")]
    [Tooltip("Used when the CharacterInfo asset leaves MoveSpeed at 0.")]
    [SerializeField] private float fallbackMoveSpeed = 4f;
    [Tooltip("Used when the CharacterInfo asset leaves Acceleration at 0.")]
    [SerializeField] private float fallbackAcceleration = 25f;

    [Header("Targeting")]
    [Tooltip("Layers an attack can damage. Player sits on Default, so leave this as Everything unless you add a Player layer.")]
    [SerializeField] private LayerMask targetLayer = ~0;
    [Tooltip("Tag the boss hunts for. The player prefab must actually carry this tag.")]
    [SerializeField] private string targetTag = "Player";
    [Tooltip("Seconds between re-scans while no target is held. Match spawns the player at runtime, so the boss has to keep looking.")]
    [SerializeField] private float targetSearchInterval = 0.5f;
    [Tooltip("Let the boss turn to face its target. On, it tracks the player in every state.")]
    [SerializeField] private bool controlFacing = true;
    [Tooltip("Tick when the source art is drawn facing LEFT. Only corrects which way the sprite is mirrored - hitboxes are unaffected.")]
    [SerializeField] private bool spriteFacesLeft;
    [Tooltip("Ignore turns while the target is within this many units horizontally, so the boss does not flip-flop when overlapping it.")]
    [SerializeField] private float facingDeadzone = 0.05f;

    [Header("Wandering (idle)")]
    [Tooltip("Seconds the boss stands and thinks before committing to a chase. Attack triggers still fire immediately.")]
    [SerializeField] private float wanderingThinkTime = 0.6f;

    [Header("Chasing")]
    [Tooltip("Fraction of MoveSpeed used while closing in. The chase ends at Bite Range.")]
    [SerializeField, Range(0f, 1f)] private float chaseMoveScale = 1f;

    [Header("Bite")]
    [SerializeField] private int biteDamage = 8;
    [Tooltip("The attack range. Wandering bites immediately inside this, and a chase ends when it reaches it.")]
    [SerializeField] private float biteRange = 10f;
    [Tooltip("Delay from the swing starting to the hitbox going live. This is the window the player has to read the bite and get out of it.")]
    [SerializeField] private float biteWindup = 0.4f;
    [SerializeField] private float biteRecovery = 0.25f;
    [Tooltip("Seconds between swings. While chaining, the boss holds the Bite state for this long so the clip keeps looping instead of dipping to Idle.")]
    [SerializeField] private float biteCooldown = 1f;
    [Tooltip("Optional. Assign BiteColliderBox to damage by trigger overlap. Left empty, the Hitbox Offset/Size below are used as a one-frame overlap test instead.")]
    [SerializeField] private DamageTriggerBox biteHitbox;
    [Tooltip("How long the bite hitbox stays live, in seconds. Only used when Bite Hitbox is assigned.")]
    [SerializeField] private float biteActiveTime = 0.12f;
    [Tooltip("Fallback hitbox, used only when Bite Hitbox is empty.")]
    [SerializeField] private Vector2 biteHitboxOffset = new Vector2(0.9f, 0f);
    [SerializeField] private Vector2 biteHitboxSize = new Vector2(1.4f, 1.2f);
    [Tooltip("Whether a bite staggers the victim. Off by default - a light nip should not interrupt.")]
    [SerializeField] private bool biteInterrupts;

    [Header("Scratch")]
    [SerializeField] private int scratchDamage = 6;
    [Tooltip("The close-range attack. Checked before Bite, so inside this distance the boss swipes instead of biting.")]
    [SerializeField] private float scratchRange = 7f;
    [Tooltip("Delay from the swipe starting to the hitbox going live. This is the window the player has to read the scratch and get out of it.")]
    [SerializeField] private float scratchWindup = 0.3f;
    [SerializeField] private float scratchRecovery = 0.2f;
    [Tooltip("Seconds before the boss may scratch again. While this is running, an in-range player gets bitten instead, so the two attacks alternate.")]
    [SerializeField] private float scratchCooldown = 1.5f;
    [Tooltip("How often a running chase re-checks whether the player has come inside Scratch Range.")]
    [SerializeField] private float scratchChaseCheckInterval = 3f;
    [Tooltip("Optional. Assign ScratchColliderBox to damage by trigger overlap. Left empty, the Hitbox Offset/Size below are used as a one-frame overlap test instead.")]
    [SerializeField] private DamageTriggerBox scratchHitbox;
    [Tooltip("How long the scratch hitbox stays live, in seconds. Only used when Scratch Hitbox is assigned.")]
    [SerializeField] private float scratchActiveTime = 0.12f;
    [Tooltip("Fallback hitbox, used only when Scratch Hitbox is empty.")]
    [SerializeField] private Vector2 scratchHitboxOffset = new Vector2(1f, 0f);
    [SerializeField] private Vector2 scratchHitboxSize = new Vector2(1.6f, 1.4f);
    [Tooltip("Whether a scratch staggers the victim.")]
    [SerializeField] private bool scratchInterrupts;

    [Header("Scratch Shockwaves")]
    [Tooltip("The projectile fired by a scratch. Leave empty to disable shockwaves entirely.")]
    [SerializeField] private Shockwave shockwavePrefab;
    [Tooltip("How many go out per scratch.")]
    [SerializeField] private int shockwaveCount = 2;
    [Tooltip("Seconds between one shockwave and the next.")]
    [SerializeField] private float shockwaveGap = 0.15f;
    [Tooltip("Delay from the swipe starting to the FIRST shockwave. Independent of Scratch Windup, so the waves and the claws can be timed separately.")]
    [SerializeField] private float shockwaveDelay = 0.3f;
    [Tooltip("Horizontal travel speed. Aimed once at spawn and never re-aimed.")]
    [SerializeField] private float shockwaveSpeed = 10f;
    [SerializeField] private int shockwaveDamage = 4;
    [Tooltip("Spawn point relative to the boss. X mirrors with facing, so enter it as if facing right.")]
    [SerializeField] private Vector2 shockwaveSpawnOffset = new Vector2(1.5f, 0f);
    [Tooltip("Whether a shockwave staggers the victim.")]
    [SerializeField] private bool shockwaveInterrupts;

    [Header("Pounce (leap across the arena)")]
    [SerializeField] private int pounceDamage = 14;
    [Tooltip("Only leaps when the target is at least this far away.")]
    [SerializeField] private float pounceMinRange = 20f;
    [Tooltip("Upper bound, so the boss does not try to clear the whole level in one jump.")]
    [SerializeField] private float pounceMaxRange = 40f;
    [SerializeField] private float pounceWindup = 0.35f;
    [Tooltip("How high above the launch point the arc peaks.")]
    [SerializeField] private float pounceApexHeight = 6f;
    [Tooltip("Lands at a random offset up to this far either side of the player's x. Wider than Damage Radius, so a leap can genuinely miss.")]
    [SerializeField] private float pounceLandRadius = 7f;
    [Tooltip("On landing, anything within this radius takes the hit.")]
    [SerializeField] private float pounceDamageRadius = 4f;
    [SerializeField] private float pounceRecovery = 0.5f;
    [SerializeField] private float pounceCooldown = 10f;
    [Tooltip("A full-body landing staggers the victim by default.")]
    [SerializeField] private bool pounceInterrupts = true;
    [SerializeField] private string verticalVelocityParameter = "VerticalVelocity";
    [SerializeField] private string isGroundedParameter = "IsGround";

    [Header("Accumulate (desperation attack)")]
    [Tooltip("Wandering auto-triggers Accumulate once HP drops below this fraction of max.")]
    [SerializeField, Range(0f, 1f)] private float accumulateHealthThreshold = 0.3f;
    [SerializeField] private float accumulateChargeTime = 3f;
    [SerializeField] private float accumulateReleaseWindup = 0.2f;
    [SerializeField] private float accumulateRecovery = 0.8f;
    [Tooltip("Stops the boss chain-casting once it is permanently below the HP threshold. Set very high for once per fight.")]
    [SerializeField] private float accumulateCooldown = 8f;
    [SerializeField] private int accumulateDamage = 30;
    [SerializeField] private Vector2 accumulateHitboxOffset = new Vector2(1.2f, 0f);
    [SerializeField] private Vector2 accumulateHitboxSize = new Vector2(3.5f, 2.5f);
    [Tooltip("The desperation attack staggers the victim by default.")]
    [SerializeField] private bool accumulateInterrupts = true;

    [Header("Animator Parameters")]
    [Tooltip("One Bool parameter per state. Entering a state raises its bool and lowers the previous one, so exactly one is ever true. Parameters the controller does not declare are skipped.")]
    [SerializeField] private string idleParam = "Idle";
    [SerializeField] private string chaseParam = "Walking";
    [SerializeField] private string biteParam = "Tearing";
    [SerializeField] private string scratchParam = "Scratch";
    [SerializeField] private string pounceParam = "Pounce";
    [SerializeField] private string accumulateParam = "Accumulate";

    [Tooltip("On: damage lands only via AnimEvent_Hit() called from an animation event. Off: the serialized windup timers deal the damage. Turn this on once every attack clip has a contact-frame event.")]
    [SerializeField] private bool useAnimationEvents;
    [SerializeField] private string readyAnimName = "Ready";
    [Header("Audio")]
    [SerializeField] private Audio[] growlsAudio;
    [SerializeField] private Audio slashAudio;
    [SerializeField] private Audio slashHitAudio;
    [SerializeField] private Audio snarlAudio;
    [SerializeField] private Audio wrenchAttackAudio;
    [SerializeField] private Audio wrenchGroundAudio;

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

    // Used to set the state bools and to read back which parameters the controller
    // actually declares. The Animator graph decides which clip that maps to.
    private Animator animator;
    private Coroutine brainRoutine;

    // Runs alongside the Scratch state rather than inside it, so the wave timing can outlast
    // the swipe without stretching the state (and the animation) to match.
    private Coroutine shockwaveRoutine;

    private Character target;
    private float nextTargetSearchTime;
    private bool warnedNoTarget;

    private ContactFilter2D hitFilter;
    private readonly List<Collider2D> hitBuffer = new();
    private readonly HashSet<string> warnedOnce = new();
    private readonly Dictionary<string, AnimatorControllerParameterType> animParameters = new();

    // The move Wandering should break out and run. Wandering means "nothing queued".
    private WolfMove pendingMove = WolfMove.Wandering;

    // +1 = the boss is facing right. Kept separate from localScale so that art drawn
    // facing left can be mirrored without inverting every hitbox offset.
    private float facingSign = 1f;

    private float moveThrottle;
    private float lastMoveCommandTime = -Mathf.Infinity;
    private float lastExternalMoveTime = -Mathf.Infinity;

    private float lastBiteTime = -Mathf.Infinity;
    private float lastScratchTime = -Mathf.Infinity;
    private float lastPounceTime = -Mathf.Infinity;
    private float lastAccumulateTime = -Mathf.Infinity;

    // A State drives walking by calling MoveTowardsTarget() every Update. If it stops
    // calling (or forgets Halt() in Exit) the command goes stale and the boss
    // decelerates on its own instead of sliding away forever.
    private const float MoveCommandTimeout = 0.1f;

    #region Public API

    /// <summary>
    /// True only while a committed move is running. Wandering and Chasing are free
    /// states: walking is allowed in both, so neither counts as busy.
    /// </summary>
    public bool IsBusy =>
        currentMove != WolfMove.Wandering &&
        currentMove != WolfMove.Chasing;
    public bool IsDead => isDead;
    public bool IsGrounded => grounded;
    public WolfMove CurrentMove => currentMove;

    /// <summary>The tracked player, or null when none is in the scene. Does not trigger a search.</summary>
    public Character Target => target;

    /// <summary>True only while a target exists and still has HP. Attacks are gated on this.</summary>
    public bool TargetIsAlive => target != null && target.Hp > 0;

    public float MoveSpeed =>
        Info != null && Info.MoveSpeed > 0f ? Info.MoveSpeed : fallbackMoveSpeed;

    public float MoveAcceleration =>
        Info != null && Info.Acceleration > 0f ? Info.Acceleration : fallbackAcceleration;

    // Falls back to full, not empty: an unassigned CharacterInfo would otherwise read as
    // 0% health and pin the boss into Accumulate forever.
    public float HealthFraction =>
        Info != null && Info.Hp > 0 ? (float)Hp / Info.Hp : 1f;

    public bool CanBite => CanAct && BiteReady;

    public bool CanScratch => CanAct && ScratchReady;

    public bool CanPounce => CanAct && PounceReady;

    // Cooldown gates. Each move's timer is read in exactly one place, so no path can enter
    // a state whose cooldown is still running.
    private bool BiteOffCooldown => Time.time >= lastBiteTime + biteCooldown;

    private bool ScratchOffCooldown => Time.time >= lastScratchTime + scratchCooldown;

    private bool PounceOffCooldown => Time.time >= lastPounceTime + pounceCooldown;

    /// <summary>
    /// "In range AND off cooldown" - the real availability test. Both the Wandering decision
    /// table and the chase's exit condition key off these rather than off range alone,
    /// because an attack that is in range but cooling down is not a reason to stand still
    /// and wait for it.
    /// </summary>
    private bool BiteReady => DistanceToTarget <= biteRange && BiteOffCooldown;

    private bool ScratchReady => DistanceToTarget <= scratchRange && ScratchOffCooldown;

    private bool PounceReady =>
        DistanceToTarget >= pounceMinRange &&
        DistanceToTarget <= pounceMaxRange &&
        PounceOffCooldown;

    public bool CanAccumulate => CanAct && ShouldAccumulate;

    /// <summary>Hurt enough to go desperate, and off cooldown. Outranks every other trigger.</summary>
    private bool ShouldAccumulate =>
        HealthFraction < accumulateHealthThreshold &&
        Time.time >= lastAccumulateTime + accumulateCooldown;

    /// <summary>Walk toward the target. Call every frame from ApproachState.Update().</summary>
    public void MoveTowardsTarget() => SetExternalMoveCommand(DirectionToTarget);

    /// <summary>Walk away from the target. Call every frame from RetreatState.Update().</summary>
    public void MoveAwayFromTarget() => SetExternalMoveCommand(-DirectionToTarget);

    /// <summary>Clears any queued walk command and zeroes horizontal velocity.</summary>
    public void Halt()
    {
        ClearMoveCommand();
        StopMoving();
    }

    /// <summary>
    /// Convenience entry point for the shared AttackState: pounces when there is room,
    /// swipes when the player is right on top of us, otherwise bites. Boss-specific
    /// states should call Pounce()/Scratch()/Bite() directly.
    /// </summary>
    public bool Attack()
    {
        if (CanPounce)
            return Pounce();

        if (CanScratch)
            return Scratch();

        return Bite();
    }

    /// <summary>Queues a move. Returns false when busy, dead, or still on cooldown.</summary>
    public bool Bite() => TryQueue(WolfMove.Bite, BiteOffCooldown);

    public bool Scratch() => TryQueue(WolfMove.Scratch, ScratchOffCooldown);

    public bool Pounce() => TryQueue(WolfMove.Pounce, PounceOffCooldown);

    public bool Accumulate() => TryQueue(WolfMove.Accumulate, Time.time >= lastAccumulateTime + accumulateCooldown);

    /// <summary>
    /// Animation Event hook. Put this on the contact frame of Bite / Pounce /
    /// AccumulateRelease, then tick Use Animation Events so the timers stop dealing
    /// the damage themselves. The hitbox used follows whichever move is running.
    /// </summary>
    public void AnimEvent_Hit()
    {
        switch (currentMove)
        {
            case WolfMove.Bite:
                StartCoroutine(BiteContact());
                break;

            case WolfMove.Scratch:
                StartCoroutine(ScratchContact());
                break;

            case WolfMove.Pounce:
                TryHitRadius(pounceDamageRadius, pounceDamage, pounceInterrupts);
                break;

            case WolfMove.Accumulate:
                TryHit(accumulateHitboxOffset, accumulateHitboxSize, accumulateDamage, accumulateInterrupts);
                break;
        }
    }

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

        // The emitter is not nested inside the brain, so stopping the brain does not stop it.
        StopShockwaves();

        // Killing the brain mid-swing would otherwise leave a box live forever.
        if (biteHitbox != null)
            biteHitbox.Disarm();

        if (scratchHitbox != null)
            scratchHitbox.Disarm();

        Halt();

        if (!isDead && isActiveAndEnabled)
            brainRoutine = StartCoroutine(BrainRoutine());
    }

    #endregion

    #region Unity

    public override void Awake()
    {
        // Caches rb and anim, and validates CharacterInfo.
        base.Awake();

        animator = GetComponent<Animator>();

        // RequireComponent adds a default Rigidbody2D; without this the boss topples
        // over the first time it is pushed.
        if (rb != null)
            rb.freezeRotation = true;

        hitFilter = new ContactFilter2D { useTriggers = true };
        hitFilter.SetLayerMask(targetLayer);

        // Normalise the scale so the sprite and facingSign agree before the first turn.
        SetFacing(facingSign);

        InitAnimParameters();

        if (targetLayer == 0)
            Debug.LogWarning($"{name}: Target Layer is set to Nothing, so attacks will never connect.", this);
    }

    public override void Start()
    {
        // Started before base.Start() so a throw in the base class can never leave the
        // boss alive but brainless.
        
        // brainRoutine = StartCoroutine(BrainRoutine());

        // base.Start();
    }
    public override void PlayReadyAnim()
    {
        PlayAnim(readyAnimName);
    }
    
    public override void StartCharacterMatch()
    {
        // Started before base.Start() so a throw in the base class can never leave the
        // boss alive but brainless.
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
    public void Update()
    {
        UpdateAnimator();
    }
    private void UpdateAnimator()
    {
        if (anim == null)
            return;

        anim.SetBool(
            isGroundedParameter,
            IsOnGround
        );

        // PUT IT HERE
        anim.SetFloat(
            verticalVelocityParameter,
            rb.linearVelocity.y
        );
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
                case WolfMove.Chasing:
                    yield return ChasingRoutine();
                    break;

                case WolfMove.Bite:
                    yield return BiteRoutine();
                    break;

                case WolfMove.Scratch:
                    yield return ScratchRoutine();
                    break;

                case WolfMove.Pounce:
                    yield return PounceRoutine();
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
    /// The resting state: stand still on the idle animation and decide what to do next.
    /// Every transition out of here is chosen by CheckWanderingTriggers.
    /// </summary>
    private IEnumerator WanderingRoutine()
    {
        SetMove(WolfMove.Wandering);
        Halt();

        float enteredAt = Time.time;

        while (pendingMove == WolfMove.Wandering && !isDead)
        {
            if (CheckWanderingTriggers(enteredAt))
                break;

            yield return null;
        }

        Halt();
    }

    /// <summary>
    /// Everything the boss decides is decided here. Order is the priority order:
    /// desperation, then attack, then close the gap.
    /// </summary>
    private bool CheckWanderingTriggers(float enteredAt)
    {
        // Nothing to attack: stand and keep looking rather than maul a corpse or empty air.
        if (!TargetIsAlive)
        {
            AcquireTarget();
            return false;
        }

        if (ShouldAccumulate)
        {
            pendingMove = WolfMove.Accumulate;
            return true;
        }

        // Scratch outranks Bite because it is the shorter-ranged move: right on top of the
        // player it swipes. While its cooldown runs it is simply not available, so this
        // falls through to the bite rather than reserving the slot for a move that cannot fire.
        if (ScratchReady)
        {
            pendingMove = WolfMove.Scratch;
            return true;
        }

        // Available: attack straight away, no thinking pause.
        if (BiteReady)
        {
            pendingMove = WolfMove.Bite;
            return true;
        }

        // Far enough to be worth crossing the arena in one jump.
        if (PounceReady)
        {
            pendingMove = WolfMove.Pounce;
            return true;
        }

        // Nothing is available right now: think, then chase - whatever the distance.
        // Deliberately unconditional. Gating this on range left a hole where the boss was
        // too close to chase but had no attack off cooldown either, so no trigger matched
        // and it stood in Idle until a timer happened to expire.
        if (Time.time - enteredAt >= wanderingThinkTime)
        {
            pendingMove = WolfMove.Chasing;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Walks at the player until an attack actually becomes available, then hands back to
    /// Wandering, which fires it. Exiting on availability rather than on range is what keeps
    /// the boss moving through a cooldown instead of parking in Idle next to the player.
    /// Scratch is polled on a fixed interval so a player who runs in mid-chase gets swiped.
    /// </summary>
    private IEnumerator ChasingRoutine()
    {
        SetMove(WolfMove.Chasing);

        float nextScratchCheck = Time.time + scratchChaseCheckInterval;

        // pendingMove guards the loop so an external Pounce()/Scratch() can cut the chase short.
        while (!isDead && pendingMove == WolfMove.Wandering)
        {
            if (!TargetIsAlive)
                break;

            // Dropping low mid-chase outranks finishing it.
            if (ShouldAccumulate)
                break;

            // Something is off cooldown and in range - Wandering takes it from here.
            if (BiteReady || PounceReady)
                break;

            // Sampled rather than tested every frame: this is a deliberate reaction delay,
            // so the boss can be caught mid-stride instead of swiping the instant you touch
            // the edge of its range.
            if (Time.time >= nextScratchCheck)
            {
                nextScratchCheck = Time.time + scratchChaseCheckInterval;

                if (ScratchReady)
                {
                    pendingMove = WolfMove.Scratch;
                    break;
                }
            }

            // A State is steering us this frame, so do not fight it with the chase.
            if (Time.time - lastExternalMoveTime > MoveCommandTimeout)
                SetMoveCommand(DirectionToTarget * chaseMoveScale);

            yield return null;
        }

        Halt();
    }

    // Single funnel for state changes, so observers (and the test view) see every one and
    // the Animator bools stay in lockstep with the logic - exactly one is ever true.
    private void SetMove(WolfMove move)
    {
        if (currentMove == move)
            return;

        SetMoveBool(currentMove, false);
        currentMove = move;
        SetMoveBool(move, true);

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

    /// <summary>
    /// Chains swings while the player stays in range, rather than returning to Wandering
    /// between each one. Leaving and re-entering would drop Tearing and raise Idle for a
    /// few frames every swing, which reads as the animation restarting from idle.
    /// The Tearing bool stays up for the whole chain, so the clip just keeps looping.
    /// </summary>
    /// <summary>
    /// Chains swings while the player stays in range, rather than returning to Wandering
    /// between each one. Leaving and re-entering would drop Tearing and raise Idle for a
    /// few frames every swing, which reads as the animation restarting from idle.
    /// The Tearing bool stays up for the whole chain, so the clip just keeps looping.
    /// </summary>
    private IEnumerator BiteRoutine()
    {
        SetMove(WolfMove.Bite);

        do
        {
            lastBiteTime = Time.time;

            Halt();

            yield return new WaitForSeconds(biteWindup);

            if (!useAnimationEvents)
                yield return BiteContact();

            yield return new WaitForSeconds(biteRecovery);

            // Hold the state through whatever is left of the cooldown so the next swing
            // starts straight from here instead of dipping through Idle first.
            float remaining = biteCooldown - (Time.time - lastBiteTime);

            if (remaining > 0f)
                yield return new WaitForSeconds(remaining);
        }
        while (!isDead &&
               pendingMove == WolfMove.Wandering &&
               !ShouldAccumulate &&
               TargetIsAlive &&
               DistanceToTarget <= biteRange);
    }

    /// <summary>
    /// The bite's contact window. With BiteColliderBox assigned it opens a real trigger
    /// window, so a player who walks into the jaws mid-swing still gets hit; without one
    /// it falls back to a single-frame overlap test.
    /// </summary>
    private IEnumerator BiteContact()
    {
        if (biteHitbox == null)
        {
            TryHit(biteHitboxOffset, biteHitboxSize, biteDamage, biteInterrupts);
            yield break;
        }

        biteHitbox.Arm(biteDamage, biteInterrupts);

        yield return new WaitForSeconds(biteActiveTime);

        biteHitbox.Disarm();
    }

    private IEnumerator PounceRoutine()
    {
        SetMove(WolfMove.Pounce);
        lastPounceTime = Time.time;

        // Plant the feet and telegraph, so the player has a window to react.
        Halt();

        yield return new WaitForSeconds(pounceWindup);

        float flightTime = LaunchTo(ChooseLandingSpot());

        // Pure ballistics from here: no steering, no ground probing. The arc is fully
        // determined by the launch impulse, so its duration is known up front and we
        // simply wait it out while gravity brings the boss down.
        yield return new WaitForSeconds(flightTime);

        StopMoving();

        // The damage is the landing itself, so it resolves once, on touchdown.
        if (!useAnimationEvents)
            TryHitRadius(pounceDamageRadius, pounceDamage, pounceInterrupts);

        yield return new WaitForSeconds(pounceRecovery);
    }

    /// <summary>
    /// The close-range swipe. Unlike Bite this never chains: one swing, then straight back
    /// to Wandering, which re-reads the distance and picks the next move from scratch.
    /// </summary>
    private IEnumerator ScratchRoutine()
    {
        SetMove(WolfMove.Scratch);
        lastScratchTime = Time.time;

        Halt();

        // Started, not yielded on: the waves keep coming on their own schedule even after
        // the swipe itself has recovered, so Shockwave Gap is free of the state's length.
        StopShockwaves();
        AudioEventChannel.Instance.Play(snarlAudio);
        shockwaveRoutine = StartCoroutine(EmitShockwaves());

        yield return new WaitForSeconds(scratchWindup);

        if (!useAnimationEvents)
            yield return ScratchContact();

        yield return new WaitForSeconds(scratchRecovery);
    }

    /// <summary>
    /// The scratch's contact window. With ScratchColliderBox assigned it opens a real
    /// trigger window, so a player who walks into the claws mid-swing still gets hit;
    /// without one it falls back to a single-frame overlap test.
    /// </summary>
    private IEnumerator ScratchContact()
    {
        AudioEventChannel.Instance.Play(slashAudio);
        if (scratchHitbox == null)
        {
            TryHit(scratchHitboxOffset, scratchHitboxSize, scratchDamage, scratchInterrupts);
            yield break;
        }

        scratchHitbox.Arm(scratchDamage, scratchInterrupts);

        yield return new WaitForSeconds(scratchActiveTime);

        scratchHitbox.Disarm();
    }

    /// <summary>
    /// Fires the scratch's shockwaves: one after Shockwave Delay, the rest spaced by
    /// Shockwave Gap. Each is aimed as it spawns, so if the player crosses to the other
    /// side between the two, the second wave chases the new side while the first carries on.
    /// </summary>
    private IEnumerator EmitShockwaves()
    {
        if (shockwavePrefab == null || shockwaveCount <= 0)
        {
            shockwaveRoutine = null;
            yield break;
        }

        if (shockwaveDelay > 0f)
            yield return new WaitForSeconds(shockwaveDelay);

        for (int i = 0; i < shockwaveCount; i++)
        {
            SpawnShockwave();

            if (i < shockwaveCount - 1 && shockwaveGap > 0f)
                yield return new WaitForSeconds(shockwaveGap);
        }

        shockwaveRoutine = null;
    }

    private void SpawnShockwave()
    {
        // Aimed here and then locked. The wave commits to where the player is standing right
        // now, which is what makes stepping over it a real option.
        float travelDirection = DirectionToTarget;

        Vector2 origin = (Vector2)transform.position +
                         new Vector2(shockwaveSpawnOffset.x * Facing, shockwaveSpawnOffset.y);

        // No parent: the boss's localScale is mirrored for facing, and a child would inherit
        // that flip on top of the one Shockwave applies itself.
        Shockwave wave = Instantiate(shockwavePrefab, origin, Quaternion.identity);

        wave.Launch(this, travelDirection, shockwaveSpeed, shockwaveDamage, shockwaveInterrupts);
    }

    private void StopShockwaves()
    {
        if (shockwaveRoutine == null)
            return;

        StopCoroutine(shockwaveRoutine);
        shockwaveRoutine = null;
    }

    /// <summary>
    /// Desperation move: root in place, charge for accumulateChargeTime, then release
    /// one heavy hit.
    /// </summary>
    private IEnumerator AccumulateRoutine()
    {
        SetMove(WolfMove.Accumulate);
        lastAccumulateTime = Time.time;

        Halt();

        yield return new WaitForSeconds(accumulateChargeTime);


        yield return new WaitForSeconds(accumulateReleaseWindup);

        if (!useAnimationEvents)
            TryHit(accumulateHitboxOffset, accumulateHitboxSize, accumulateDamage, accumulateInterrupts);

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

    private float DistanceToTarget
    {
        get
        {
            Character current = TargetCharacter;

            if (current == null)
                return Mathf.Infinity;

            return Vector2.Distance(transform.position, current.transform.position);
        }
    }

    /// <summary>+1 when the target is to the right. Falls back to current facing when there is no target.</summary>
    private float DirectionToTarget
    {
        get
        {
            Character current = TargetCharacter;

            if (current != null)
            {
                float delta = current.transform.position.x - transform.position.x;

                if (!Mathf.Approximately(delta, 0f))
                    return Mathf.Sign(delta);
            }

            return Facing;
        }
    }

    /// <summary>
    /// +1 when the boss is facing right. Hitbox offsets mirror off this rather than off
    /// the raw localScale, so ticking Sprite Faces Left never inverts the attack boxes.
    /// </summary>
    private float Facing => facingSign;

    /// <summary>
    /// Turns to the target every physics step, in every state. Deliberately not gated on
    /// IsBusy: the boss tracks the player through its attacks too.
    /// </summary>
    private void UpdateFacing()
    {
        if (!controlFacing || isDead)
            return;

        Character current = TargetCharacter;

        if (current == null)
            return;

        float delta = current.transform.position.x - transform.position.x;

        // Deadzone stops a jitter loop when the two are stacked on the same column.
        if (Mathf.Abs(delta) < facingDeadzone)
            return;

        SetFacing(Mathf.Sign(delta));
    }

    private void SetFacing(float sign)
    {
        facingSign = sign;

        // The extra flip is the art's own orientation, not the direction we are facing.
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * sign * (spriteFacesLeft ? -1f : 1f);
        transform.localScale = scale;
    }

    private void SetExternalMoveCommand(float throttle)
    {
        lastExternalMoveTime = Time.time;
        SetMoveCommand(throttle);
    }

    // throttle is signed and normally +/-1, but the approach can scale it down.
    private void SetMoveCommand(float throttle)
    {
        if (isDead || IsBusy)
            return;

        moveThrottle = throttle;
        lastMoveCommandTime = Time.time;
    }

    private void ClearMoveCommand()
    {
        moveThrottle = 0f;
        lastMoveCommandTime = -Mathf.Infinity;
    }

    private void ApplyWalk()
    {
        if (IsBusy || isDead)
            return;

        if (Time.time - lastMoveCommandTime > MoveCommandTimeout)
            moveThrottle = 0f;

        Move(moveThrottle, MoveSpeed, MoveAcceleration);
    }

    /// <summary>
    /// Picks a spot beside the player - front or back, chosen at random - so the leap
    /// can cross over and land behind them rather than always arriving head-on.
    /// </summary>
    private Vector2 ChooseLandingSpot()
    {
        Character current = TargetCharacter;
        Vector2 origin = transform.position;

        if (current == null)
            return origin + new Vector2(Facing * pounceLandRadius, 0f);

        // Anywhere within pounceLandRadius of the player's x - in front, on top, or past
        // them. Only the x is read; the arc returns to the height it launched from.
        float landingX = current.transform.position.x +
                         Random.Range(-pounceLandRadius, pounceLandRadius);

        return new Vector2(landingX, origin.y);
    }

    /// <summary>
    /// Solves the launch velocity that arcs from here to <paramref name="landing"/> while
    /// peaking pounceApexHeight above the start, so the leap actually lands where it aimed
    /// instead of being a fixed-speed lunge.
    /// </summary>
    /// <summary>
    /// Fires one impulse that arcs from here to <paramref name="landing"/>, peaking
    /// pounceApexHeight above the launch point, and returns how long that arc lasts.
    /// Nothing touches the Rigidbody again until it is over - gravity does the rest.
    /// </summary>
    private float LaunchTo(Vector2 landing)
    {
        Vector2 origin = transform.position;
        float gravity = Mathf.Abs(Physics2D.gravity.y) * rb.gravityScale;

        if (gravity <= 0.01f)
        {
            WarnOnce(
                "~nogravity",
                "Rigidbody2D Gravity Scale is 0, so Pounce has no arc to fall through. Set it to 1"
            );
            return 0f;
        }

        // Symmetric arc: up to the apex, then back down to the launch height.
        float vy = Mathf.Sqrt(2f * gravity * Mathf.Max(pounceApexHeight, 0.01f));
        float dy = landing.y - origin.y;

        // Descending root of  dy = vy*t - 0.5*g*t^2  is the full flight time. A negative
        // discriminant means the apex sits below the landing height, so clamp to the apex.
        float discriminant = Mathf.Max(vy * vy - 2f * gravity * dy, 0f);
        float flightTime = (vy + Mathf.Sqrt(discriminant)) / gravity;

        float vx = flightTime > 0.01f
            ? (landing.x - origin.x) / flightTime
            : 0f;

        // Impulse rather than a velocity write, scaled by mass so the resulting velocity
        // is exactly (vx, vy) whatever the Rigidbody weighs.
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(vx, vy) * rb.mass, ForceMode2D.Impulse);

        return flightTime;
    }

    private bool TryHitRadius(float radius, int damage, bool isInterruptible)
    {
        int count = Physics2D.OverlapCircle(
            transform.position,
            radius,
            hitFilter,
            hitBuffer
        );

        for (int i = 0; i < count; i++)
        {
            Character victim = hitBuffer[i].GetComponentInParent<Character>();

            if (victim == null || victim == this)
                continue;

            Hit(victim, damage, isInterruptible);
            return true;
        }

        return false;
    }

    private bool TryHit(Vector2 offset, Vector2 size, int damage, bool isInterruptible)
    {
        Vector2 center = (Vector2)transform.position +
                         new Vector2(offset.x * Facing, offset.y);

        int count = Physics2D.OverlapBox(center, size, 0f, hitFilter, hitBuffer);

        for (int i = 0; i < count; i++)
        {
            Character victim = hitBuffer[i].GetComponentInParent<Character>();

            if (victim == null || victim == this)
                continue;

            Hit(victim, damage, isInterruptible);
            return true;
        }

        return false;
    }

    private string ParamFor(WolfMove move) => move switch
    {
        WolfMove.Wandering => idleParam,
        WolfMove.Chasing => chaseParam,
        WolfMove.Bite => biteParam,
        WolfMove.Scratch => scratchParam,
        WolfMove.Pounce => pounceParam,
        WolfMove.Accumulate => accumulateParam,
        _ => null
    };

    private void SetMoveBool(WolfMove move, bool value)
    {
        string parameter = ParamFor(move);

        if (!IsBoolParameter(parameter))
            return;

        animator.SetBool(parameter, value);
    }

    /// <summary>
    /// Checked against the cached parameter list rather than called blind, because
    /// SetBool on a parameter the controller does not declare warns on every single call.
    /// </summary>
    private bool IsBoolParameter(string parameter)
    {
        if (string.IsNullOrEmpty(parameter))
            return false;

        if (animator == null || animator.runtimeAnimatorController == null)
        {
            WarnOnce("~nocontroller", "the Animator has no controller assigned, so no animation will ever play");
            return false;
        }

        if (!animParameters.TryGetValue(parameter, out AnimatorControllerParameterType type))
        {
            WarnOnce(parameter, $"the controller has no parameter named '{parameter}', so that state will not animate");
            return false;
        }

        if (type != AnimatorControllerParameterType.Bool)
        {
            WarnOnce(parameter, $"parameter '{parameter}' is a {type}, but it needs to be a Bool");
            return false;
        }

        return true;
    }

    private void WarnOnce(string key, string message)
    {
        // HashSet.Add returns false when already present, so each problem logs once.
        if (!warnedOnce.Add(key))
            return;

        Debug.LogWarning($"{name}: {message}.", this);
    }

    /// <summary>
    /// Caches the declared parameters, clears every move bool, then raises the current
    /// one. The raise is needed because SetMove early-returns on an unchanged state, so
    /// the starting Wandering state would otherwise never assert its bool.
    /// </summary>
    private void InitAnimParameters()
    {
        animParameters.Clear();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                animParameters[parameter.name] = parameter.type;
            }
        }

        foreach (WolfMove move in System.Enum.GetValues(typeof(WolfMove)))
        {
            SetMoveBool(move, false);
        }

        SetMoveBool(currentMove, true);
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            position + new Vector2(scratchHitboxOffset.x * facing, scratchHitboxOffset.y),
            scratchHitboxSize
        );

        // Where the shockwaves are born, and the line they travel along.
        Vector2 spawn = position + new Vector2(shockwaveSpawnOffset.x * facing, shockwaveSpawnOffset.y);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(spawn, 0.2f);
        Gizmos.DrawLine(spawn, spawn + new Vector2(facing * 3f, 0f));

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(position, pounceDamageRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(
            position + new Vector2(accumulateHitboxOffset.x * facing, accumulateHitboxOffset.y),
            accumulateHitboxSize
        );

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(position, scratchRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(position, biteRange);
        Gizmos.DrawWireSphere(position, pounceMinRange);
        Gizmos.DrawWireSphere(position, pounceMaxRange);

        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(position, position + Vector2.down * groundCheckDistance);
    }
}
