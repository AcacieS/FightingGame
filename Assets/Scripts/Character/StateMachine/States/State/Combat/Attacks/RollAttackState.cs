using UnityEngine;

public class RollAttackState : NearAttackState
{
    //Movement are same as Movement State but for now we will just copy
    [Header("Roll Movement")]
    [SerializeField] private float movementDistance = 3f;

    [Tooltip("If enabled, the character moves away from the target.")]
    [SerializeField] private bool moveAwayFromTarget = false;

    [SerializeField] private float movementSpeed = 3f;
    [SerializeField] private float movementAcceleration = 3f;

    [Header("Debug")]
    [ReadOnly, SerializeField] private float movementDirection;

    private Vector3 startPosition;
    private float direction;
    private bool hasAttack;

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
    }

    public override void Play()
    {
        // =========================
        // MOVEMENT
        // =========================

        movementDirection = moveAwayFromTarget
            ? -direction
            : direction;

        AI.Character.Move(
            movementDirection,
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
            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Roll Attack");

        AI.Character.StopMoving();

        base.Exit();
    }
}