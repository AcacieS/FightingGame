using UnityEngine;

public class JumpCenterState : ActionState
{
    [Header("Jump Center")]
    
    [SerializeField] private float apexLimit = 1f;

    private Rigidbody2D rb;

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Apex");

        rb = Context.Self.Rb;

    }

    public override void Play()
    {
        if (rb.linearVelocity.y <= apexLimit)
        {
            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Apex");
    }
}