using UnityEngine;

public class HunterWolf : Enemy
{
    private CharacterController ai;

    public CharacterController AI => ai;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float moveSpeedWhileJumping = 6f;
    [SerializeField] private float moveSpeedWhileRecharging = 2f;
    [SerializeField] private float moveSpeedWhileShooting = 1f;
    [SerializeField] private float acceleration = 20f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    Timer WalkAway;
    Timer Shoot;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        ai = GetComponent<CharacterController>();
    }

    void Update()
    {
        //ai.Target <= 
    }

    private void Movement()
    {
        float targetSpeed = moveInput.x * moveSpeed;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }
}
