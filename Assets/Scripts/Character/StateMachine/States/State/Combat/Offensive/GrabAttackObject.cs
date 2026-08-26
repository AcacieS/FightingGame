using UnityEngine;

public class GrabAttackObject : MonoBehaviour
{
    private Rigidbody2D rb;

    private Character owner;
    private Character target;

    private float direction;
    private float speed;
    private float acceleration;

    private float returnSpeed;
    private float returnAcceleration;

    private float attackRange;
    private float maxDistance;
    private int damage;

    private LayerMask charactersLayer;

    private Vector2 startPosition;

    private bool isReturning;
    private bool hasHit;
    private Rigidbody2D grabbedRb;

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
        float direction,
        float speed,
        float acceleration,
        float returnSpeed,
        float returnAcceleration,
        float attackRange,
        float maxDistance,
        int damage,
        LayerMask charactersLayer)
    {
        this.owner = owner;
        this.target = target;

        this.direction = direction;

        this.speed = speed;
        this.acceleration = acceleration;

        this.returnSpeed = returnSpeed;
        this.returnAcceleration = returnAcceleration;

        this.attackRange = attackRange;
        this.maxDistance = maxDistance;
        this.damage = damage;

        this.charactersLayer = charactersLayer;

        startPosition = transform.position;

        isReturning = false;
        hasHit = false;
    }

    private void FixedUpdate()
    {
        Move();

        if (isReturning)
            return;

        CheckHit();

        // The target wasn't hit, so return after
        // reaching the maximum distance.
        CheckMaximumDistance();
    }

    private void Move()
    {
        if (rb == null)
            return;

        float targetSpeed = direction * speed;

        float newSpeed = Mathf.MoveTowards(
            rb.linearVelocity.x,
            targetSpeed,
            acceleration * Time.fixedDeltaTime
        );

        rb.linearVelocity = new Vector2(
            newSpeed,
            rb.linearVelocity.y
        );
    }
    
    private void CheckHit()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
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

            // Don't hit the character that created the attack.
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

            //character.Hurt(damage);
            if (character is Player player)
            {
                player.Stun();
            }

            grabbedRb = character.GetComponent<Rigidbody2D>();

            if (grabbedRb != null)
            {
                grabbedRb.linearVelocity = Vector2.zero;
                grabbedRb.bodyType = RigidbodyType2D.Kinematic;
            }

            character.transform.SetParent(transform);
            StartReturn();

            return;
        }
    }

    private void CheckMaximumDistance()
    {
        float distanceMoved = Mathf.Abs(
            transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= maxDistance)
        {
            Debug.Log(
                $"{name}: Maximum distance reached. Returning."
            );

            StartReturn();
        }
    }

    private void StartReturn()
    {
        if (isReturning)
            return;
        Debug.Log("Should Return");
        isReturning = true;

        direction = -direction;

        speed = returnSpeed;
        acceleration = returnAcceleration;
    }

    public bool HasReturned(float threshold = 0.1f)
    {
        if (!isReturning)
            return false;

        float distanceFromStart = Mathf.Abs(
            transform.position.x -
            startPosition.x
        );
        bool hasReturned = distanceFromStart <= threshold;
        if (hasReturned)
        {
            ReleaseTarget();
        }
        return hasReturned;
    }
    private void ReleaseTarget()
    {
        if (grabbedRb != null)
        {
            grabbedRb.bodyType = RigidbodyType2D.Dynamic;
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