using UnityEngine;

public class RollAttackState : NearAttackState
{
    [SerializeField] private CooldownRequirement coolDownRequirement;
    //Movement are same as Movement State but for now we will just copy
    [Header("Roll Movement")]
    [SerializeField] private float movementDistance = 3f;
    

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementAcceleration = 3f;
    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;

    [Header("Debug")]
    [ReadOnly, SerializeField] private float _movementDirection;

    private Vector3 startPosition;
    private float direction;
    private bool hasAttack;
    private Collider2D characterCollider;

    public override void Enter()
    {
        Debug.Log("AI → Roll Attack");

        // NearAttackState.Enter()
        // will also call ActionState.Enter()
        base.Enter();

        hasAttack = false;

        // Stop any previous movement.
        AI.Character.Move(0);

        startPosition = AI.Character.transform.position;

        // Direction toward the target.
        direction = Context.DirectionSign;
        characterCollider = AI.Character.GetComponent<Collider2D>();

        if (characterCollider == null)
        {
            Debug.LogError($"{name}: Character has no Collider2D.");
        }
    }

    public override void Play()
    {
        // =========================
        // MOVEMENT
        // =========================

        _movementDirection = moveAwayFromTarget
            ? -direction
            : direction;
        // Check wall BEFORE moving.
        if (IsTouchingWall(_movementDirection))
        {
            Debug.Log($"{name}: Hit wall → Finish Movement");
            RequestRootDecision();
            return;
        }
        AI.Character.Move(
            _movementDirection,
            movementSpeed,
            movementAcceleration
        );

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
            coolDownRequirement.Initialize();
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

        int hitCount = characterCollider.Cast(
            castDirection,
            new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = wallLayer,
                useTriggers = false
            },
            hits,
            wallCheckDistance
        );

        return hitCount > 0;
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Roll Attack");

        AI.Character.StopMoving();

        base.Exit();
    }
}