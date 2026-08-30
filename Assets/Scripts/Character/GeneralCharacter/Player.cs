using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private string jumpAnimName = "Jump";

    [Header("Block")]
    [SerializeField] private float blockDuration = 0.5f;
    [SerializeField] private float blockCooldown = 1f;
    [SerializeField] private string blockAnimName = "Block";

    [Header("Stun")]
    [SerializeField] private float defaultStunDuration = 2f;
    [SerializeField] private string stunAnimName = "Stunned";

    [ReadOnly, SerializeField] private bool isStunned;
    [Header("Bottle")]
    [SerializeField] private BottleInfo bottleInfo;
    public BottleInfo BottleInfo => bottleInfo;

    private Coroutine stunCouroutine;

    public bool IsStunned => isStunned;

    [Header("Animator Parameters")]
    [SerializeField] private string isMovingParameter = "IsMoving";
    [SerializeField] private string isGroundedParameter = "IsGrounded";
    [SerializeField] private string verticalVelocityParameter = "VerticalVelocity";
    [SerializeField] private string horizontalSpeedParameter = "HorizontalSpeed";

    [ReadOnly, SerializeField]
    private bool isBlocking;

    [ReadOnly, SerializeField]
    private bool blockOnCooldown;

    private Coroutine blockCoroutine;
    private Coroutine cooldownCoroutine;

    private Vector2 moveInput;

    public bool IsBlocking => isBlocking;

    [SerializeField] private PlayerAttack playerAttack;

    public override void Awake()
    {
        base.Awake();
        if (playerAttack == null) playerAttack = GetComponent<PlayerAttack>();
    }
    

    private void Update()
    {
        CheckGround();
        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && !isStunned && !controlsLocked && IsOnGround)
        {
            Jump();
        }
    }

    public void OnBlock(InputValue value)
    {
        // Debug.Log($"OnBlock called: {value.isPressed}");

        if (value.isPressed)
        {
            TryBlock();
        }
        else
        {
            // Debug.Log("Q Released");
            StopBlock();
        }
    }
    
    public void OnShoot(InputValue value)
    {
        // Debug.Log($"OnBlock called: {value.isPressed}");

        if (value.isPressed)
        {
            TryShoot();
        }
    }
    
    private Coroutine throwRoutine;
    private bool bottleReleased;

    public bool IsThrowing => throwRoutine != null;

    public void TryShoot()
    {
        if (isStunned || controlsLocked || isBlocking || throwRoutine != null
            || (playerAttack != null && playerAttack.IsAttacking))
            return;

        if (!bottleInfo.ThrowBottle())
            return;

        throwRoutine = StartCoroutine(ThrowRoutine());


    /*
        PlayAnim(bottleInfo.AnimName);

        float facingDirection =
            Mathf.Sign(transform.localScale.x);

        bottleInfo.SpawnBottle(
            this,
            facingDirection
        );
        */
    }

    private IEnumerator ThrowRoutine()
    {
        bottleReleased = false;
        PlayAnim(bottleInfo.AnimName);

        float start = Time.time;
        while(Time.time < start + 2f)
        {
            if (isStunned || controlsLocked)
            {
                throwRoutine = null;
                yield break; //interrupted mid throw
            }

            if (IsAnimFinished(bottleInfo.AnimName))
                break;
            
            yield return null;
        }

        if (!bottleReleased)
        {
            Debug.LogWarning($"{name}: BottleAttack clip has no realease event, spawning at anim end");
            ReleaseBottle();
        }

        PlayAnim("Idle");
        throwRoutine = null;
    }

    //called by the animation event on the bottle clip via PlayerAnimationEvents
    public void ReleaseBottle()
    {
        if (bottleReleased || throwRoutine == null)
            return;

        bottleReleased = true;

        float facingDirection = Mathf.Sign(transform.localScale.x);
        bottleInfo.SpawnBottle(this, facingDirection);
    }

    [ReadOnly, SerializeField] private bool controlsLocked;
    public bool IsControlsLocked => controlsLocked;
    public override void Start()
    {
        base.Start();
        controlsLocked = true;
    }
    public override void PlayReadyAnim()
    {
        //Do Ready Animation
        controlsLocked = true;
        StopMoving();
        PlayAnim("Ready");
    }
    
    public override void StartCharacterMatch()
    {
        //TODO Allow Player to move and all
        controlsLocked = false;
        PlayAnim("Idle");
    }

    private void TryBlock()
    {
        if(isStunned && controlsLocked) return;

        if (isBlocking)
            return;

        if (blockOnCooldown)
        {
            Debug.Log($"{name}: Block is on cooldown.");
            return;
        }

        Debug.Log($"{name}: Start Block");
        PlayAnim(blockAnimName);
        blockCoroutine = StartCoroutine(BlockRoutine());
    }

    private IEnumerator BlockRoutine()
    {
        isBlocking = true;

        PlayAnim("Block");

        yield return new WaitForSeconds(blockDuration);

        StopBlock();
    }

    private void StopBlock()
    {
        if (!isBlocking)
            return;

        Debug.Log($"{name}: Stop Block");
        PlayAnim("Idle");
        isBlocking = false;

        if (blockCoroutine != null)
        {
            StopCoroutine(blockCoroutine);
            blockCoroutine = null;
        }

        // Start cooldown.
        if (cooldownCoroutine != null)
        {
            StopCoroutine(cooldownCoroutine);
        }

        cooldownCoroutine = StartCoroutine(BlockCooldownRoutine());

        // TODO: Return to appropriate animation/state.
        PlayAnim("Idle");
    }

    private IEnumerator BlockCooldownRoutine()
    {
        blockOnCooldown = true;

        yield return new WaitForSeconds(blockCooldown);

        blockOnCooldown = false;
        cooldownCoroutine = null;

        Debug.Log($"{name}: Block ready.");
    }

    private void Movement()
    {
        if (isStunned || controlsLocked) return;

        float targetSpeed = moveInput.x * Info.MoveSpeed;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            Info.Acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }

    private void Jump()
    {
        PlayAnim(jumpAnimName);
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }
    
    private void CheckGround()
    {
        Debug.DrawRay(
            transform.position,
            Vector3.down * groundCheckDistance,
            IsOnGround ? Color.green : Color.red
        );
    }

    public override bool Hurt(
        int damage,
        bool isInterruptible = false,
        float stunDuration = 0f)
    {
        if (isBlocking)
        {
            Debug.Log($"{name}: Blocked attack!");
            return false;
        }

        base.Hurt(damage, isInterruptible, stunDuration); //moved here so anims play

        if (stunDuration != 0f)
        {
            Stun(stunDuration);
        }        

        return true;
    }

    public override void Die()
    {
        if(IsDead) 
            return;
        
        base.Die();

        controlsLocked = true;
        StopMoving();
        StopAllCoroutines();
    }

    private void UpdateAnimator()
    {
        if (anim == null)
            return;

        anim.SetFloat(
            horizontalSpeedParameter,
            Mathf.Abs(rb.linearVelocity.x)
        );

        anim.SetBool(
            isGroundedParameter,
            IsOnGround
        );

        anim.SetFloat(
            verticalVelocityParameter,
            rb.linearVelocity.y
        );
    }

    public void Stun()
    {
        Stun(defaultStunDuration);
    }

    public void Stun(float duration)
    {
        if (stunCouroutine != null)
            StopCoroutine(stunCouroutine);
        
        stunCouroutine = StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        if (isBlocking)
            StopBlock();
        
        StopMoving();
        PlayAnim(stunAnimName);

        yield return new WaitForSeconds(duration);

        isStunned = false;
        stunCouroutine = null;
        PlayAnim("Idle");
    }

    [ContextMenu("Stun Test")]
    private void StunTest()
    {
        Hurt(5, false, 2f);
    }
    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMatchStart += BottleInfo.InitalizeBottle;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnMatchStart -= BottleInfo.InitalizeBottle;
    }
}