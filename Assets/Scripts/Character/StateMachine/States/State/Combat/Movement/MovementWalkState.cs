using UnityEngine;

public class MovementWalkState : ActionState
{
    [Header("Movement")]
    [SerializeField] private float movementDistance = 3f;

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;
    [SerializeField] private float movementSpeed = 2f;
    [SerializeField] private float movementAcceleration = 2f;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float wallCheckDistance = 0.05f;
    [Header("Movement Fail Safe")]
    [SerializeField] private float notMovingTimeout = 0.5f;
    [SerializeField] private float minMovementSpeed = 0.1f;

    private float notMovingTimer;

    [ReadOnly, SerializeField] private float movementDirection;

    private Vector3 startPosition;
    private float direction;

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
        notMovingTimer = 0f;

        // base.Enter();

        //AI.Character.Move(0);

        // _startPosition = AI.Character.transform.position;
        // _direction = Context.DirectionSign;
        
        // characterCollider = AI.Character.GetComponent<Collider2D>();

        // if (characterCollider == null)
        // {
        //     Debug.LogError($"{name}: Character has no Collider2D.");
        // }
        // startMoving = true;
        // _movementDirection = moveAwayFromTarget
        //     ? -_direction
        //     : _direction;
    }

    public override void Play()
    {
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
        // Fail-safe: detect if we're stuck
        if (Mathf.Abs(AI.Character.Rb.linearVelocity.x) < minMovementSpeed)
        {
            notMovingTimer += Time.deltaTime;

            if (notMovingTimer >= notMovingTimeout)
            {
                Debug.LogWarning(
                    $"{name}: Character failed to move for " +
                    $"{notMovingTimer:F2}s → exiting movement state."
                );

                RequestDecision();
                return;
            }
        }
        else
        {
            // Character is actually moving, so reset the timer.
            notMovingTimer = 0f;
        }

        // if (Mathf.Approximately(_direction, 0f))
        // {
        //     Debug.LogError("Context got 0, choose old direction");
        //     _direction = Context.LastDirectionSign;
        //     if(Mathf.Approximately(_direction, 0f))
        //     {
        //         _direction = -1;
        //     }
        //     _movementDirection = moveAwayFromTarget
        //     ? -_direction
        //     : _direction;
        // }
        
        // if(Mathf.Approximately(_movementDirection, 0f))
        // {
        //     Debug.LogError("movement Dir is 0");
        // }
        // // Check wall BEFORE moving.
        // if (IsTouchingWall(_movementDirection))
        // {
        //     Debug.Log($"{name}: Hit wall → Finish Movement");
        //     RequestDecision();
        //     return;
        // }

        // if (_overrideMovementSettings)
        // {
        //     AI.Character.Move(
        //         _movementDirection,
        //         _movementSpeed,
        //         _movementAcceleration
        //     );
            
        // }
        // else
        // {
        //     AI.Character.Move(_movementDirection);
        // }
            
        
        //Check movement distance.
        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= movementDistance)
        {
            RequestDecision();
        }
    }
    
    // private void FixedUpdate()
    // {
        // if (startMoving)
        // {
        //     if (_overrideMovementSettings)
        //     {
        //         Debug.LogWarning("override movement: _movementSpeed: "+_movementSpeed+", _movementAcceleration"+_movementAcceleration);
        //         AI.Character.Move(
        //             _movementDirection,
        //             _movementSpeed,
        //             _movementAcceleration
        //         );
        //     }
        //     else
        //     {
        //         Debug.LogWarning("not override movement");
        //         AI.Character.Move(_movementDirection);
        //     }
        //     float distanceMoved = Mathf.Abs(
        //         AI.Character.transform.position.x -
        //         _startPosition.x
        //     );

        //     if (distanceMoved >= _movementDistance)
        //     {
        //         RequestDecision();
        //     }
        // }
    //}
    
    // private bool IsActuallyMoving()
    // {
    //     if(Mathf.Abs(AI.Character.Rb.linearVelocity.x) > 0.01f)
    //     {
    //         notMoving = false;
    //     }
    //     else
    //     {
    //         notMoving = true;
            
    //     }
    //     return notMoving;
    // }

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
        Debug.Log("AI → Exit Movement");
        AI.Character.StopMoving();
        base.Exit();
    }
}