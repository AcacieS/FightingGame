using UnityEngine;

public class RetreatState : State
{
    [SerializeField] private float retreatDistance = 3f;
    [ReadOnly, SerializeField] private float retreatDirection;

    private Vector3 startPosition;

    public override void Enter()
    {
        Debug.Log("AI → Retreat");
        AI.Character.Move(0);
        startPosition = AI.Character.transform.position;
    }

    public override void Play()
    {
        Character target = AI.Context.Target;

        if (target == null)
            return;

        float directionToTarget = Mathf.Sign(
            AI.Context.Direction
        );

        retreatDirection = -directionToTarget;

        AI.Character.Move(retreatDirection);

        AI.Character.LookAt(target);

        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= retreatDistance)
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