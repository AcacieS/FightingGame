using UnityEngine;

public class WalkState : NearAttackState
{
    // =========================
    // Roll Movement
    // =========================

    [Header("Movement")]
    [SerializeField] private float movementDistance = 3f;

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementAcceleration = 3f;

   

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
    private Vector3 startPosition;
    private float direction;

    private Collider2D characterCollider;
    
    public override void Enter()
    {
        Debug.Log("AI → Roll Attack");

        base.Enter();

        AI.Character.Move(0);
        
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

            RequestDecision();
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
        // FINISH
        // =========================

        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= movementDistance)
        {
            RequestDecision();
        }
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
        Debug.Log("AI → Exit Exit Attack");
        AI.Character.StopMoving();

        Context.Self.transform.rotation =
            Quaternion.identity;
        base.Exit();
    }
}