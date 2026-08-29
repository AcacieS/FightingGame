using System.Collections;
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

    public override void Awake()
    {
        base.Awake();
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
        if (value.isPressed && !isStunned && IsOnGround)
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

    private void TryBlock()
    {
        if(isStunned) return;

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
        if (isStunned) return;

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
}