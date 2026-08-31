using UnityEngine;

public class JumpState : ActionState
{
    [Header("Jump")]
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float apexLimit = 1f;

    private Rigidbody2D rb;
    private bool hasJumped;

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Jump");

        rb = Context.Self.Rb;

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D not found.");
            return;
        }

        hasJumped = true;

        // Start the jump.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );
    }

    public override void Play()
    {
        if (!hasJumped || rb == null)
            return;

        // The character has reached the peak and is now falling.
        if (rb.linearVelocity.y <= apexLimit)
        {
            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Jump");
        hasJumped = false;
    }
}