using UnityEngine;

public class ApproachState : State
{
    public override void Enter()
    {
        Debug.Log("AI → Approach");
    }

    public override void Update()
    {
        Character target = AI.Context.Target;

        if (target == null)
        {
            Debug.LogError("target is null");
            return;
        }
            

        Transform targetTransform = target.transform;

        // Direction toward the opponent
        float direction = Mathf.Sign(
            targetTransform.position.x - AI.Character.transform.position.x
        );

        // Move toward opponent
        Vector3 movement = Vector3.right *
                           direction *
                           AI.Character.Info.MoveSpeed *
                           Time.deltaTime;

        AI.Character.transform.position += movement;

        // Stop approaching when close enough
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