using UnityEngine;

public class RollAttackState : NearAttackState
{
    // =========================
    // Roll Movement
    // =========================

    [Header("Roll Movement")]
    [SerializeField] private Audio grandmaRoll;
    [SerializeField] private float movementDistance = 3f;

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementAcceleration = 3f;

    // =========================
    // Roll Rotation
    // =========================

    [Header("Roll Rotation")]
    [Tooltip("How many degrees the character rotates per unit of movement.")]
    [SerializeField] private float rotationPerUnit = 180f;

    [Tooltip("If enabled, rotation is based on actual movement speed.")]
    [SerializeField] private bool rotateBasedOnSpeed = true;

    // =========================
    // Wall Detection
    // =========================

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;

    // =========================
    // Debug
    // =========================

    [Header("Debug")]
    [ReadOnly, SerializeField] private float movementDirection;
    [ReadOnly, SerializeField] private float currentRotationSpeed;

    private Vector3 startPosition;
    private float direction;
    private bool hasAttack;
    [ReadOnly, SerializeField] private Quaternion initialRotation = Quaternion.identity;

    private Collider2D characterCollider;
    
    public override void Enter()
    {
        Debug.Log("AI → Roll Attack");

        base.Enter();

        hasAttack = false;

        AI.Character.Move(0);
        
        AudioEventChannel.Instance.Play(grandmaRoll);

        startPosition = AI.Character.transform.position;

        direction = Context.DirectionSign;

        characterCollider =
            AI.Character.GetComponent<Collider2D>();

        if (characterCollider == null)
        {
            Debug.LogError(
                $"{name}: Character has no Collider2D."
            );
        }
    }

    public override void Play()
    {
        // =========================
        // MOVEMENT DIRECTION
        // =========================

        movementDirection = moveAwayFromTarget
            ? -direction
            : direction;

        // =========================
        // WALL
        // =========================

        if (IsTouchingWall(movementDirection))
        {
            Debug.Log(
                $"{name}: Hit wall → Finish Roll"
            );

            RequestRootDecision();
            return;
        }

        // =========================
        // MOVEMENT
        // =========================

        AI.Character.Move(
            movementDirection,
            movementSpeed,
            movementAcceleration
        );

        // =========================
        // ROTATION
        // =========================

        RotateRoll();

        // =========================
        // ATTACK
        // =========================

        if (!hasAttack && Attack())
        {
            hasAttack = true;
        }

        // =========================
        // FINISH
        // =========================

        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= movementDistance)
        {
            AudioEventChannel.Instance.Stop(grandmaRoll);
            RequestDecision();
        }
    }

    private void RotateRoll()
    {
        float speed = Mathf.Abs(
            AI.Character.GetComponent<Rigidbody2D>()
                .linearVelocity.x
        );

        if (rotateBasedOnSpeed)
        {
            currentRotationSpeed =
                speed * rotationPerUnit;
        }
        else
        {
            currentRotationSpeed =
                movementSpeed * rotationPerUnit;
        }

        // Moving right → clockwise
        // Moving left → counter-clockwise
        float rotationDirection = -Mathf.Sign(movementDirection);

        AI.Character.transform.Rotate(
            0f,
            0f,
            rotationDirection *
            currentRotationSpeed *
            Time.deltaTime
        );
    }

    private bool IsTouchingWall(float direction)
    {
        if (characterCollider == null)
            return false;

        if (Mathf.Approximately(direction, 0f))
            return false;

        Vector2 castDirection = new Vector2(
            Mathf.Sign(direction),
            0f
        );

        RaycastHit2D[] hits = new RaycastHit2D[5];

        ContactFilter2D filter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = wallLayer,
            useTriggers = false
        };

        int hitCount = characterCollider.Cast(
            castDirection,
            filter,
            hits,
            wallCheckDistance
        );

        return hitCount > 0;
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Roll Attack");

        AI.Character.StopMoving();

        AI.Character.transform.rotation = initialRotation;

        base.Exit();
    }
}