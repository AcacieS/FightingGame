using UnityEngine;

public class BallAttackState : ActionState
{
    [Header("Bounce")]
    [SerializeField] private float bounceSpeed = 8f;
    [SerializeField] private float bounceAcceleration = 20f;
    [SerializeField] private int nbBounce = 1;
    private int currentNbBounce;

    [Header("Detection")]
    [SerializeField] private Collider2D characterCollider;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float detectionDistance = 0.1f;

    [Header("Debug")]
    [ReadOnly, SerializeField] private Vector2 direction;

    private Rigidbody2D rb;
    private float originalGravityScale;
    public override void Enter()
    {
        base.Enter();

        rb = Context.Self.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D not found.");
            return;
        }
        currentNbBounce = 0;
        // Save normal gravity.
        originalGravityScale = rb.gravityScale;

        // Disable gravity for the bounce attack.
        rb.gravityScale = 0f;


        if (characterCollider == null)
        {
            characterCollider = Context.Self.GetComponent<Collider2D>();
        }

        // Pick the first random direction.
        direction = GetRandomDirection();

        SetVelocity();
    }

    public override void Play()
    {
        if (rb == null)
            return;

        DetectCollision();

        // Keep the bounce moving.
        SetVelocity();
    }

    private void SetVelocity()
    {
        Vector2 targetVelocity = direction * bounceSpeed;

        rb.linearVelocity = Vector2.MoveTowards(
            rb.linearVelocity,
            targetVelocity,
            bounceAcceleration * Time.deltaTime
        );
    }

    private void DetectCollision()
    {
        if (characterCollider == null)
            return;
        
        Bounds bounds = characterCollider.bounds;

        // Cast the collider in the current direction.
        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            direction,
            detectionDistance,
            environmentLayer
        );

        if (!hit.collider)
            return;

        Debug.Log($"{name}: Bounce hit {hit.collider.name}");

        currentNbBounce++;
        Debug.Log("currentNbBounce: "+currentNbBounce);
        if (currentNbBounce >= nbBounce)
        {
            RequestDecision();
            return;
        }
        Bounce(hit.normal);
    }

    private void Bounce(Vector2 normal)
    {
        // Reflect the current direction off the surface.
        Vector2 newDirection = Vector2.Reflect(direction, normal);

        // Normalize to make sure speed stays consistent.
        newDirection.Normalize();
        
        direction = newDirection;

        Debug.Log(
            $"{name}: New bounce direction = {direction}"
        );
    }

    private Vector2 GetRandomDirection()
    {
        // Random angle around the character.
        float angle = Random.Range(0f, 360f);

        Vector2 randomDirection = new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        );

        return randomDirection.normalized;
    }

    public override void Exit()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = originalGravityScale;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (characterCollider == null)
            return;

        Gizmos.DrawRay(
            characterCollider.bounds.center,
            direction * detectionDistance
        );
    }
}