using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    private Vector2 moveInput;

    public override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        CheckGround();
    }

    private void FixedUpdate()
    {
        Movement();
    }

    public void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
        // Debug.Log($"OnMove: {moveInput}");
    }

    public void OnJump(InputValue value)
    {
        if (value.isPressed && IsGrounded())
        {
            Jump();
        }
    }

    private void Movement()
    {
        float targetSpeed = moveInput.x * Info.MoveSpeed;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, Info.Acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }
    public void PlayReadyAnim()
    {
        //TODO: Play Ready Anim Logic;
    }
    public void StartMatch()
    {
        //TODO: Player now can do whatever it can do, like moving and all
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);
        return hit.collider != null;
    }

    private void CheckGround()
    {
        Debug.DrawRay(
            transform.position,
            Vector3.down * groundCheckDistance,
            IsGrounded() ? Color.green : Color.red
        );
    }
    public override bool Hurt(int damage, bool isInterruptible = false, bool isStun = false)
    {
        //TODO: Add the check if Player is Blocking
        // if (Block)
        // {
        //     Debug.Log($"{name} blocked the attack!");
        //     return false;
        // }

        if(isStun) Stun();
        base.Hurt(damage, isInterruptible, isStun);
        return true;
    }
    public void Stun()
    {
        //TODO: Should interrupt Player
        Move(0);
    }
}
