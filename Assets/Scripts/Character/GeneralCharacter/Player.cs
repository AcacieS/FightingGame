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
        if (value.isPressed && IsOnGround)
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
    public void PlayReadyAnim()
    {
        //Do Ready Animation
    }
    
    public void StartPlayerMatch()
    {
        //TODO Allow Player to move and all
    }

    private void TryBlock()
    {
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
        bool isStun = false)
    {
        if (isBlocking)
        {
            Debug.Log($"{name}: Blocked attack!");
            return false;
        }

        if (isStun)
        {
            Stun();
        }

        base.Hurt(damage, isInterruptible, isStun);

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
        Move(0);
    }
}