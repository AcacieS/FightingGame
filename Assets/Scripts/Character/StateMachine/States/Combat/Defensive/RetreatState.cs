using UnityEngine;

public class RetreatState : State
{
    public override void Enter()
    {
        Debug.Log("AI → Retreat");
    }

    public override void Update()
    {
        // Move away from target

        if (AI.Context.SelfHp > 30)
        {
            RequestDecision();
        }
    }
}