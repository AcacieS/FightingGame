using UnityEngine;

public class MovementState : ActionState
{
    [Header("Movement")]
    [SerializeField] private float _movementDistance = 3f;

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;

    [Tooltip("If enabled, the character uses the custom speed and acceleration.")]
    [SerializeField] private bool _overrideMovementSettings = false;

    [SerializeField] private float _movementSpeed = 3f;
    [SerializeField] private float _movementAcceleration = 3f;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;

    [ReadOnly, SerializeField] private float _movementDirection;

    private Vector3 _startPosition;
    private float _direction;

    private Collider2D characterCollider;

    public override void Enter()
    {
        if (moveAwayFromTarget)
        {
            Debug.Log("AI → Retreat Movement State");
        }
        else
        {
            Debug.Log("AI → Approach Movement State");
        }

        base.Enter();

        AI.Character.Move(0);

        _startPosition = AI.Character.transform.position;
        _direction = Context.DirectionSign;

        characterCollider = AI.Character.GetComponent<Collider2D>();

        if (characterCollider == null)
        {
            Debug.LogError($"{name}: Character has no Collider2D.");
        }
    }

    public override void Play()
    {
        _movementDirection = moveAwayFromTarget
            ? -_direction
            : _direction;

        // Check wall BEFORE moving.
        if (IsTouchingWall(_movementDirection))
        {
            Debug.Log($"{name}: Hit wall → Finish Movement");
            RequestRootDecision();
            return;
        }

        // Move.
        if (_overrideMovementSettings)
        {
            AI.Character.Move(
                _movementDirection,
                _movementSpeed,
                _movementAcceleration
            );
        }
        else
        {
            AI.Character.Move(_movementDirection);
        }

        // Check movement distance.
        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            _startPosition.x
        );

        if (distanceMoved >= _movementDistance)
        {
            RequestRootDecision();
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
        Debug.Log("AI → Exit Movement");

        AI.Character.StopMoving();
    }
}