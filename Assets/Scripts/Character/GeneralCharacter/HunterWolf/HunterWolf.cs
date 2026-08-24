using UnityEngine;

public class HunterWolf : Enemy
{
    private CharacterControllerA ai;

    public CharacterControllerA AI => ai;

    [Header("State")]
    [SerializeField] private State AimState;
    [SerializeField] private State FowardState;
    [SerializeField] private State IdleState;
    [SerializeField] private State KickState;
    [SerializeField] private State LauchTrapState;
    [SerializeField] private State RechargeState;
    [SerializeField] private State ShootState;
    [SerializeField] private State StunState;

    [Header("Movement")]
    [SerializeField] private float acceleration = 20f;
    float moveSpeed = 0;

    [Header("Jump")]
    [SerializeField] private float groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Combat Distance")]
    [SerializeField] private float tooFarDistance = 8f;
    [SerializeField] private float tooCloseDistance = 2f;

    [SerializeField] Transform gunEndPoint;
    public Transform GunEndPoint => gunEndPoint;

    Timer WalkAway;
    Timer Timer;
    int directionFoward = 1;
    bool canInitiateOtherState = true;
    public bool CanInitiateOtherState { get => canInitiateOtherState; set => canInitiateOtherState = value; }
    float distance;
    public float Distance => distance;

    private Rigidbody2D rb;
    private Vector2 moveInput;

    private void Awake()
    {
        ai = GetComponent<CharacterControllerA>();

        AimState.Initialize(ai);
        FowardState.Initialize(ai);
        IdleState.Initialize(ai);
        KickState.Initialize(ai);
        LauchTrapState.Initialize(ai);
        RechargeState.Initialize(ai);
        StunState.Initialize(ai);
    }


    private void Update()
    {
        if (ai == null || ai.Target == null)
            return;

        Movement();

        distance = Vector2.Distance(transform.position, ai.Target.transform.position);

        if (!canInitiateOtherState)
            return;

        if (distance >= tooFarDistance)
        {
            ai.ChangeState(FowardState);
            return;
        }
        else if (distance <= tooCloseDistance)
        {
            ai.ChangeState(KickState);
            return;
        }
        else
        {
            ai.ChangeState(AimState);
            return;
        }
    }


    private void Movement()
    {
        float targetSpeed = moveInput.x * moveSpeed * Mathf.Sign(transform.position.x - ai.Target.transform.position.x) * directionFoward;

        float newSpeed = Mathf.MoveTowards(rb.linearVelocity.x, targetSpeed, acceleration * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newSpeed, rb.linearVelocity.y);
    }

    public void Jump(float jumpForce)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    public void ChangeSpeed(float newMoveSpeed, int newDirectionFoward = -1)
    {
        moveSpeed = newMoveSpeed;
        directionFoward = newDirectionFoward;
    }
}
