using UnityEngine;

public class BallAttackState : ActionState
{
    [Header("Bounce")]
    [SerializeField] private float bounceSpeed = 8f;
    [SerializeField] private int nbBounce = 1;
    private int currentNbBounce;

    [Header("Direction")]
    [Tooltip("How strongly the initial direction is biased toward the target.")]
    [SerializeField, Range(0f, 1f)] private float targetDirectionBias = 0.75f;

    [Tooltip("Maximum random angle added to the direction toward the target.")]
    [SerializeField] private float randomAngle = 45f;

    [Header("Rotation")]
    [Tooltip("How many degrees the character rotates per unit of movement.")]
    [SerializeField] private float rotationPerUnit = 180f;

    [Tooltip("Rotate based on the actual movement speed.")]
    [SerializeField] private bool rotateBasedOnSpeed = true;

    [Header("Detection")]
    [SerializeField] private Collider2D characterCollider;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float detectionDistance = 0.1f;
    [SerializeField] private CooldownRequirement cooldownRequirement;

    [Header("Debug")]
    [ReadOnly, SerializeField] private Vector2 direction;
    [ReadOnly, SerializeField] private float currentRotationSpeed;

    private Rigidbody2D rb;
    private float originalGravityScale;

    // Store this so we can restore it when the state exits.

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

        // Save gravity.
        originalGravityScale = rb.gravityScale;

        // Disable gravity during attack.
        rb.gravityScale = 0f;

        if (characterCollider == null)
        {
            characterCollider =
                Context.Self.GetComponent<Collider2D>();
        }

        // Get a direction toward the target,
        // with some randomness.
        direction = GetTargetBiasedDirection();

        SetVelocity();
    }

    public override void Play()
    {
        if (rb == null)
            return;

        if (DetectCollision())
            return;

        // Keep moving.
        SetVelocity();

        // Rotate the character while moving.
        RotateBall();
    }

    private void SetVelocity()
    {
        rb.linearVelocity = direction * bounceSpeed;
    }

    // =========================================================
    // DIRECTION
    // =========================================================

    private Vector2 GetTargetBiasedDirection()
    {
        if (Context.Target == null)
            return GetRandomDirection();

        Vector2 toTarget =
            Context.Target.transform.position -
            Context.Self.transform.position;

        if (toTarget.sqrMagnitude <= 0.001f)
            return GetRandomDirection();

        Vector2 targetDirection = toTarget.normalized;

        // Random direction.
        Vector2 randomDirection = GetRandomDirection();

        // Bias between random and target.
        Vector2 finalDirection = Vector2.Lerp(
            randomDirection,
            targetDirection,
            targetDirectionBias
        );

        return finalDirection.normalized;
    }

    private Vector2 RotateVector(Vector2 vector, float angle)
    {
        float radians = angle * Mathf.Deg2Rad;

        float cos = Mathf.Cos(radians);
        float sin = Mathf.Sin(radians);

        return new Vector2(
            vector.x * cos - vector.y * sin,
            vector.x * sin + vector.y * cos
        );
    }

    private Vector2 GetRandomDirection()
    {
        float angle = Random.Range(0f, 360f);

        return new Vector2(
            Mathf.Cos(angle * Mathf.Deg2Rad),
            Mathf.Sin(angle * Mathf.Deg2Rad)
        ).normalized;
    }

    // =========================================================
    // ROTATION
    // =========================================================

    private void RotateBall()
    {
        float speed = rb.linearVelocity.magnitude;

        if (rotateBasedOnSpeed)
        {
            currentRotationSpeed =
                speed * rotationPerUnit;
        }
        else
        {
            currentRotationSpeed =
                bounceSpeed * rotationPerUnit;
        }

        // Rotate around its own Z axis.
        //
        // The sign determines clockwise/counter-clockwise.
        float rotationDirection =
            Mathf.Sign(rb.linearVelocity.x);

        if (Mathf.Approximately(rotationDirection, 0f))
            rotationDirection = 1f;

        Context.Self.transform.Rotate(
            0f,
            0f,
            -rotationDirection *
            currentRotationSpeed *
            Time.deltaTime,
            Space.Self
        );
    }

    // =========================================================
    // COLLISION
    // =========================================================

    private bool DetectCollision()
    {
        if (characterCollider == null)
            return false;

        Bounds bounds = characterCollider.bounds;

        RaycastHit2D hit = Physics2D.BoxCast(
            bounds.center,
            bounds.size,
            0f,
            direction,
            detectionDistance,
            environmentLayer
        );

        if (!hit.collider)
            return false;

        Debug.Log(
            $"{name}: Bounce hit {hit.collider.name}"
        );

        currentNbBounce++;

        if (currentNbBounce >= nbBounce)
        {
            RequestDecision();
            return true;
        }

        Bounce(hit.normal);

        return false;
    }

    private void Bounce(Vector2 normal)
    {
        // Natural reflection.
        Vector2 reflectedDirection =
            Vector2.Reflect(direction, normal).normalized;

        // Direction toward target.
        Vector2 targetDirection =
            (Context.Target.transform.position -
            Context.Self.transform.position).normalized;

        // Blend the reflected direction with the target direction.
        Vector2 newDirection = Vector2.Lerp(
            reflectedDirection,
            targetDirection,
            targetDirectionBias
        );

        direction = newDirection.normalized;

        Debug.Log(
            $"{name}: Bounce → " +
            $"Reflected: {reflectedDirection} | " +
            $"Target: {targetDirection} | " +
            $"Final: {direction}"
        );
    }

    // =========================================================
    // EXIT
    // =========================================================

    public override void Exit()
    {
        cooldownRequirement.Initialize();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = originalGravityScale;
        }
        
        // Return character to the rotation it had before the attack.
        Context.Self.transform.rotation =
            Quaternion.identity;
        Debug.Log("should return normal rotation");
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