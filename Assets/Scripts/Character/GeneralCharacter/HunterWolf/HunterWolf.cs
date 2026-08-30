using UnityEngine;

public class HunterWolf : Enemy
{
    private AIController ai;

    public AIController AI => ai;

    [Header("State")]
    [SerializeField] private State AimState;
    [SerializeField] private State FowardState;
    [SerializeField] private State IdleState;
    [SerializeField] private State KickState;
    [SerializeField] private State LauchTrapState;
    [SerializeField] private State RechargeState;
    [SerializeField] private State ShootState;
    [SerializeField] private State StunState;
    public Animator Animator => anim;

    [Header("Movement")]
    [SerializeField] private float acceleration = 20f;
    float moveSpeed = 0;


    [Header("Combat Distance")]
    [SerializeField] private float tooFarDistance = 8f;
    [SerializeField] private float tooCloseDistance = 2f;

    [SerializeField] Transform gunEndPoint;
    public Transform GunEndPoint => gunEndPoint;
    [SerializeField] Transform lauchEndPoint;
    public Transform LauchEndPoint => lauchEndPoint;

    int directionFoward = 1;
    bool canInitiateOtherState = true;
    public bool CanInitiateOtherState { get => canInitiateOtherState; set => canInitiateOtherState = value; }
    float distance;
    public float Distance => distance;
    bool isAiming;
    public bool IsAiming { get => isAiming; set => isAiming = value; }
    bool hasABullet = true;
    public bool HasABullet { get => hasABullet; set => hasABullet = value; }

    bool controlsLocked = true;

    [SerializeField] float timeWaitBetweenState;
    Timer timerWaitBetweenState;
    public Timer TimerWaitBetweenState => timerWaitBetweenState;
    [SerializeField] float timeWaitBetweenTrap;
    Timer timerWaitBetweenTrap;

    public override void Awake()
    {
        base.Awake();
        ai = GetComponent<AIController>();

        timerWaitBetweenState = new Timer(timeWaitBetweenState);
        timerWaitBetweenTrap = new Timer(timeWaitBetweenTrap);

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
        if (controlsLocked)
            return;

        if (ai == null || ai.Target == null)
            return;

        if (Context.Instance.SelfState is DeadHunterState)
            return;

        if (Mathf.Sign(transform.position.x - ai.Target.transform.position.x) != transform.localScale.x)
        {
            transform.localScale = new Vector3(Mathf.Sign(transform.position.x - ai.Target.transform.position.x), 1, 1);
        }
        Movement();

        distance = Vector2.Distance(transform.position, ai.Target.transform.position);

        if (!canInitiateOtherState)
            return;

        if (!timerWaitBetweenState.IsOver())
            return;

        if (distance >= tooFarDistance && !isAiming)
        {
            if (!hasABullet)
            {
                ai.ChangeState(RechargeState);
            }
            ai.ChangeState(FowardState);
            return;
        }
        else if (distance <= tooCloseDistance)
        {
            ai.ChangeState(KickState);
            return;
        }
        else if (!isAiming)
        {
            float LauchOrNo = Random.Range(1, 3);
            if (LauchOrNo == 1 && timerWaitBetweenTrap.IsOver())
            {
                timerWaitBetweenTrap.Restart();
                ai.ChangeState(LauchTrapState);
            }
            else
            {
                if (hasABullet)
                {
                    ai.ChangeState(AimState);
                }
                else
                {
                    ai.ChangeState(RechargeState);
                }
            }

            return;
        }
    }


    private void Movement()
    {
        float targetSpeed = moveSpeed * Mathf.Sign(ai.Target.transform.position.x - transform.position.x) * directionFoward;

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

    public void StartPlay()
    {
        controlsLocked = false;
        timerWaitBetweenState.Restart();
        timerWaitBetweenTrap.Restart();
    }

    public void ReadyPlay()
    {
        timerWaitBetweenState.Restart();
        timerWaitBetweenTrap.Restart();
    }
}
