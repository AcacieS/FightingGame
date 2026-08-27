using UnityEngine;

public class FallState : ActionState
{
    private Rigidbody2D rb;

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Fall");

        rb = Context.Self.Rb;

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D not found.");
        }
    }

    public override void Play()
    {
        if (Context.Self.IsOnGround)
        {
            Debug.Log("AI → Landed");

            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Fall");
    }
}