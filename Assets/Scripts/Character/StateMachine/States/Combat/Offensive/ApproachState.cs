using UnityEngine;

public class ApproachState : State
{
    public override void Enter()
    {
        Debug.Log("AI → Approach");
    }

    public override void Update()
    {
        // Move toward target
        // AI.Character.MoveTowards(AI.Target);

        if (AI.Context.Distance <= 2f)
        {
            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Approach");
    }
}