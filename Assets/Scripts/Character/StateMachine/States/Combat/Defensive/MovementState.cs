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
        Debug.Log("AI → Retreat");

        AI.Character.Move(0);

        _startPosition = AI.Character.transform.position;
        _direction = Context.DirectionSign;
    }

    public override void Play()
    {
        Character target = AI.Context.Target;

        if (target == null)
            return;

        _movementDirection = moveAwayFromTarget ? -_direction: _direction;
        if (_overrideMovementSettings)
        {
            AI.Character.Move(_direction, _movementSpeed, _movementAcceleration);
        }
        else
        {
            AI.Character.Move(_movementDirection);
        }

        AI.Character.LookAt(target);

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