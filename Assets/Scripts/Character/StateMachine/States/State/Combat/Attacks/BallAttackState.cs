using UnityEngine;

public class BallAttackState : NearAttackState
{
    // =========================================================
    // Bounce
    // =========================================================

    [Header("Bounce")]
    [SerializeField] private float bounceSpeed = 8f;
    [SerializeField] private int nbBounce = 1;

    private int currentNbBounce;

    // =========================================================
    // Direction
    // =========================================================

    [Header("Direction")]
    [Tooltip("How strongly the initial direction is biased toward the target.")]
    [SerializeField, Range(0f, 1f)]
    private float targetDirectionBias = 0.75f;

    [Tooltip("Maximum random angle added to the direction toward the target.")]
    [SerializeField]
    private float randomAngle = 45f;

    // =========================================================
    // Rotation
    // =========================================================

    [Header("Rotation")]
    [Tooltip("How many degrees the character rotates per unit of movement.")]
    [SerializeField]
    private float rotationPerUnit = 180f;

    [Tooltip("Rotate based on the actual movement speed.")]
    [SerializeField]
    private bool rotateBasedOnSpeed = true;

    [SerializeField] private Audio rollSFX;

    // =========================================================
    // Detection
    // =========================================================

    [Header("Detection")]
    [SerializeField] private Collider2D characterCollider;
    [SerializeField] private LayerMask environmentLayer;
    [SerializeField] private float detectionDistance = 0.1f;

    // =========================================================
    // Debug
    // =========================================================

    [Header("Debug")]
    [ReadOnly, SerializeField] private Vector2 direction;
    [ReadOnly, SerializeField] private float currentRotationSpeed;

    // =========================================================
    // Runtime
    // =========================================================

    private Rigidbody2D rb;
    private float originalGravityScale;
    private bool hasHitCharacter;

    // =========================================================
    // ENTER
    // =========================================================

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Ball Attack");

        hasHitCharacter = false;
        currentNbBounce = 0;

        AudioEventChannel.Instance.Play(rollSFX);

        rb = Context.Self.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                $"{name}: Rigidbody2D not found."
            );
            return;
        }

        // Save gravity.
        originalGravityScale = rb.gravityScale;

        // Ball should travel freely.
        rb.gravityScale = 0f;

        // Get collider.
        if (characterCollider == null)
        {
            characterCollider =
                Context.Self.GetComponent<Collider2D>();
        }

        if (characterCollider == null)
        {
            Debug.LogError(
                $"{name}: Collider2D not found."
            );
            return;
        }

        // Calculate initial direction.
        direction = GetTargetBiasedDirection();

        // Start moving immediately.
        SetVelocity();
    }

    // =========================================================
    // PLAY
    // =========================================================

    public override void Play()
    {
        if (rb == null)
            return;

        if (hasHitCharacter)
            return;

        // -----------------------------------------------------
        // Make sure velocity stays constant.
        // -----------------------------------------------------

        SetVelocity();

        // -----------------------------------------------------
        // Detect collision.
        // -----------------------------------------------------

        if (DetectCollision())
            return;

        // -----------------------------------------------------
        // Rotate.
        // -----------------------------------------------------

        RotateBall();
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private void SetVelocity()
    {
        rb.linearVelocity =
            direction.normalized * bounceSpeed;
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

        Vector2 targetDirection =
            toTarget.normalized;

        Vector2 randomDirection =
            GetRandomDirection();

        Vector2 finalDirection =
            Vector2.Lerp(
                randomDirection,
                targetDirection,
                targetDirectionBias
            );

        return finalDirection.normalized;
    }

    private Vector2 GetRandomDirection()
    {
        float angle =
            Random.Range(0f, 360f);

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
        float speed =
            rb.linearVelocity.magnitude;

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

        // -----------------------------------------------------
        // Direction of the actual ball movement.
        // -----------------------------------------------------

        Vector2 castDirection =
            rb.linearVelocity.normalized;

        if (castDirection.sqrMagnitude <= 0.001f)
            return false;

        Bounds bounds =
            characterCollider.bounds;

        // -----------------------------------------------------
        // 1. Check CHARACTER first.
        // -----------------------------------------------------

        RaycastHit2D characterHit =
            Physics2D.BoxCast(
                bounds.center,
                bounds.size,
                0f,
                castDirection,
                detectionDistance,
                charactersLayer
            );

        if (characterHit.collider != null)
        {
            Character character =
                characterHit.collider
                    .GetComponentInParent<Character>();

            if (character != null &&
                character != Context.Self)
            {
                Debug.Log(
                    $"{name}: Ball hit {character.name}"
                );

                // Same attack logic as NearAttackState.
                bool isHurt =
                    character.Hurt(
                        damage,
                        _doesInterrupt,
                        _stunDuration
                    );

                AttackResult =
                    isHurt
                        ? AttackResult.Success
                        : AttackResult.Blocked;

                hasHitCharacter = true;

                // STOP the ball immediately.
                rb.linearVelocity = Vector2.zero;

                AudioEventChannel.Instance.Stop(
                    rollSFX
                );

                // Leave this state.
                RequestDecision();

                return true;
            }
        }

        // -----------------------------------------------------
        // 2. Check ENVIRONMENT.
        // -----------------------------------------------------

        RaycastHit2D environmentHit =
            Physics2D.BoxCast(
                bounds.center,
                bounds.size,
                0f,
                castDirection,
                detectionDistance,
                environmentLayer
            );

        if (environmentHit.collider != null)
        {
            Debug.Log(
                $"{name}: Ball hit environment"
            );

            currentNbBounce++;

            // No more bounces.
            if (currentNbBounce >= nbBounce)
            {
                rb.linearVelocity =
                    Vector2.zero;

                AudioEventChannel.Instance.Stop(
                    rollSFX
                );

                RequestDecision();

                return true;
            }

            // Bounce.
            Bounce(
                environmentHit.normal
            );

            return false;
        }

        return false;
    }

    // =========================================================
    // BOUNCE
    // =========================================================

    private void Bounce(Vector2 normal)
    {
        // Reflect current movement.
        Vector2 reflectedDirection =
            Vector2.Reflect(
                direction,
                normal
            ).normalized;

        // Try to bias the bounce toward target.
        Vector2 targetDirection =
            reflectedDirection;

        if (Context.Target != null)
        {
            Vector2 toTarget =
                Context.Target.transform.position -
                Context.Self.transform.position;

            if (toTarget.sqrMagnitude > 0.001f)
            {
                targetDirection =
                    toTarget.normalized;
            }
        }

        Vector2 newDirection =
            Vector2.Lerp(
                reflectedDirection,
                targetDirection,
                targetDirectionBias
            );

        direction =
            newDirection.normalized;

        SetVelocity();

        AudioEventChannel.Instance.Stop(
            rollSFX
        );

        AudioEventChannel.Instance.Play(
            rollSFX
        );

        Debug.Log(
            $"{name}: Bounce → " +
            $"Reflected: {reflectedDirection} | " +
            $"New Direction: {direction}"
        );
    }

    // =========================================================
    // EXIT
    // =========================================================

    public override void Exit()
    {
        Debug.Log(
            "AI → Exit Ball Attack"
        );

        AudioEventChannel.Instance.Stop(
            rollSFX
        );

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;

            rb.gravityScale =
                originalGravityScale;
        }

        Context.Self.transform.rotation =
            Quaternion.identity;

        base.Exit();
    }

    // =========================================================
    // GIZMOS
    // =========================================================

    // private void OnDrawGizmosSelected()
    // {
    //     if (characterCollider == null)
    //         return;

    //     Vector2 gizmoDirection = direction;

    //     if (rb != null &&
    //         Application.isPlaying &&
    //         rb.linearVelocity.sqrMagnitude > 0.001f)
    //     {
    //         gizmoDirection =
    //             rb.linearVelocity.normalized;
    //     }

    //     Gizmos.DrawRay(
    //         characterCollider.bounds.center,
    //         gizmoDirection *
    //         detectionDistance
    //     );
    // }
}