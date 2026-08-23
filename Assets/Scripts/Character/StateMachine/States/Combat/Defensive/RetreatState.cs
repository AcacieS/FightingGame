using UnityEngine;

public class RetreatState : State
{
    public override void Enter()
    {
        Debug.Log("AI → Retreat");
    }

    public override void Update()
    {
        Character target = AI.Context.Target;

        if (target == null)
            return;

        // Direction from AI to target
        float directionToTarget =
            Mathf.Sign(
                target.transform.position.x -
                AI.Character.transform.position.x
            );

        // Move in the opposite direction
        float retreatDirection = -directionToTarget;

        AI.Character.Move(retreatDirection);

        // Face the opponent while retreating
        AI.Character.LookAt(target);

        // Stop retreating when healthy enough
        if (AI.Context.SelfHp > 30f)
        {
            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Retreat");
    }
}