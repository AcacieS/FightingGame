using UnityEngine;

public class MovementState : State
{
    [SerializeField] private float _movementDistance = 3f;
    [Tooltip("If enabled, the character moves away from the target. If disabled, the character moves toward the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;  

    [Tooltip("If enabled, this state overrides movement value. If disabled, it uses the speed and acceleration from the CharacterInfo.")]
    [SerializeField] private bool _overrideMovementSettings = false;

    [SerializeField] private float _movementSpeed = 3f;
    [SerializeField] private float _movementAcceleration = 3f;

    [ReadOnly, SerializeField] private float _movementDirection;

    private Vector3 _startPosition;
    private float _direction;

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
    }

    public override void Play()
    {

        _movementDirection = moveAwayFromTarget ? -_direction: _direction;
        if (_overrideMovementSettings)
        {
            AI.Character.Move(_movementDirection, _movementSpeed, _movementAcceleration);
        }
        else
        {
            AI.Character.Move(_movementDirection);
        }

        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            _startPosition.x
        );

        if (distanceMoved >= _movementDistance)
        {
            RequestRootDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Retreat");
        AI.Character.StopMoving();
    }
}