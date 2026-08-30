using UnityEngine;

public class GrabAttackObject : MonoBehaviour
{
    private Rigidbody2D rb;

    private Character owner;
    private Character target;

    private Vector2 direction;

    private float speed;
    private float acceleration;

    private float returnSpeed;
    private float returnAcceleration;

    private float attackRange;
    private float maxDistance;
    private int damage;
    private bool _doesInterrupt;
    private float _stunDuration;

    private LayerMask charactersLayer;

    private Vector2 startPosition;

    private bool isReturning;
    private bool hasHit;

    private Rigidbody2D grabbedRb;

    private bool hasGrabPlayer;
    private bool playerHasBlocked;
    private Audio hookGrabAudio;

    public bool PlayerHasBlocked => playerHasBlocked;
    public bool HasGrabPlayer => hasGrabPlayer;
    public bool IsReturning => isReturning;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                $"{name}: GrabAttackObject requires a Rigidbody2D.",
                this
            );
        }
    }

    public void Initialize(
        Character owner,
        Character target,

        float facingDirection,
        float angle,

        float speed,
        float acceleration,

        float returnSpeed,
        float returnAcceleration,

        float attackRange,
        float maxDistance,

        int damage,
        bool _doesInterrupt,
        float _stunDuration,
        LayerMask charactersLayer,
        Audio hookGrabAudio)
    {
        this.owner = owner;
        this.target = target;

        this.speed = speed;
        this.acceleration = acceleration;

        this.returnSpeed = returnSpeed;
        this.returnAcceleration = returnAcceleration;

        this.attackRange = attackRange;
        this.maxDistance = maxDistance;

        this.damage = damage;
        this._doesInterrupt = _doesInterrupt;
        this._stunDuration = _stunDuration;
        this.charactersLayer = charactersLayer;
        this.hookGrabAudio = hookGrabAudio;
        startPosition = transform.position;

        isReturning = false;
        hasHit = false;
        hasGrabPlayer = false;
        playerHasBlocked = false;

        // Convert the angle relative to the character's
        // facing direction into a world-space direction.
        float radians = angle * Mathf.Deg2Rad;

        direction = new Vector2(
            Mathf.Cos(radians) * facingDirection,
            Mathf.Sin(radians)
        ).normalized;

        Debug.Log(
            $"{name}: Grab direction = {direction}"
        );
    }
    private Vector2 previousPosition;
    private void FixedUpdate()
    {
        previousPosition = transform.position;
        Move();

        if (isReturning)
        {
            CheckReturn();
            return;
        }

        CheckHit();
        CheckMaximumDistance();
    }

    private void Move()
    {
        if (rb == null)
            return;

        Vector2 targetVelocity =
            direction * speed;

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            targetVelocity,
            acceleration * Time.fixedDeltaTime
        );
    }

    private void CheckHit()
    {
        Collider2D[] hitEnemies =
            Physics2D.OverlapCircleAll(
                transform.position,
                attackRange,
                charactersLayer
            );

        foreach (Collider2D enemy in hitEnemies)
        {
            Character character =
                enemy.GetComponentInParent<Character>();

            if (character == null)
                continue;

            // Don't hit the owner.
            if (character == owner)
                continue;

            // Only hit the intended target.
            if (character != target)
                continue;

            if (hasHit)
                return;

            hasHit = true;

            Debug.Log(
                $"{name}: Grab attack hit {character.name}"
            );

            if (character is Player player)
            {
                
                bool isHurt =
                    player.Hurt(
                        damage,
                        _doesInterrupt, 
                        _stunDuration
                    );

                if (isHurt)
                {
                    hasGrabPlayer = true;
                    AudioEventChannel.Instance.Play(hookGrabAudio);
                    grabbedRb =
                        character.GetComponent<Rigidbody2D>();

                    if (grabbedRb != null)
                    {
                        grabbedRb.linearVelocity =
                            Vector2.zero;

                        grabbedRb.bodyType =
                            RigidbodyType2D.Kinematic;
                    }

                    character.transform.SetParent(
                        transform
                    );
                }
                else
                {
                    playerHasBlocked = true;
                }
            }

            StartReturn();

            return;
        }
    }

    private void CheckMaximumDistance()
    {
        float distanceMoved =
            Vector2.Distance(
                transform.position,
                startPosition
            );

        if (distanceMoved >= maxDistance)
        {
            Debug.Log(
                $"{name}: Maximum distance reached. Returning."
            );

            StartReturn();
        }
    }
    private bool hasReturned;
    private void StartReturn()
    {
        if (isReturning)
            return;

        Debug.Log("Grab → Return");

        isReturning = true;
        hasReturned = false;

        // Return along the exact opposite direction.
        direction = -direction;

        speed = returnSpeed;
        acceleration = returnAcceleration;
    }
    private void CheckReturn()
    {
        float distanceFromStart = Vector2.Distance(
            transform.position,
            startPosition
        );

        float threshold =
            Mathf.Max(
                0.1f,
                returnSpeed * Time.fixedDeltaTime
            );

        if (distanceFromStart <= threshold)
        {
            Debug.Log($"{name}: Returned to start!");

            ReleaseTarget();
            hasReturned = true;
        }
    }
    public bool HasReturned()
{
    return hasReturned;
}
    // public bool HasReturned(float threshold = 0.1f)
    // {
    //     if (!isReturning)
    //         return false;

    //     float distanceFromStart =
    //         Vector2.Distance(
    //             transform.position,
    //             startPosition
    //         );

    //     if (distanceFromStart <= threshold)
    //     {
    //         ReleaseTarget();
    //         return true;
    //     }

    //     return false;
    // }

    private void ReleaseTarget()
    {
        if (grabbedRb != null)
        {
            grabbedRb.bodyType =
                RigidbodyType2D.Dynamic;

            grabbedRb = null;
        }

        if (target != null)
        {
            target.transform.SetParent(null);
        }
    }

    private void OnDisable()
    {
        ReleaseTarget();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(
            transform.position,
            attackRange
        );
    }
}