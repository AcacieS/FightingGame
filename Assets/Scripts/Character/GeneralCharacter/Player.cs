using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Character
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float acceleration = 20f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    public override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
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
        float targetSpeed = moveInput.x * moveSpeed;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
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
}
