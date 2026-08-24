using UnityEngine;

public class ApproachState : State
{
    [SerializeField] private float approachDistance = 3f;

    private Vector3 startPosition;

    public override void Enter()
    {
        Debug.Log("AI → Approach");

        startPosition = AI.Character.transform.position;
        AI.Character.Move(0);
    }

    public override void Play()
    {
        Character target = AI.Context.Target;

        if (target == null)
            return;

        float direction = Mathf.Sign(
            target.transform.position.x -
            AI.Character.transform.position.x
        );

        AI.Character.Move(direction);

        float distanceMoved = Mathf.Abs(
            AI.Character.transform.position.x -
            startPosition.x
        );

        if (distanceMoved >= approachDistance)
        {
            RequestRootDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Approach");
        AI.Character.StopMoving();
    }
}