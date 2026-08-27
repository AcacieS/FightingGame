using UnityEngine;

public class LaunchBallAttackState : ActionState
{
    [Header("Launch")]
    [SerializeField] private float targetHeight = 4f;
    [SerializeField] private float launchSpeed = 8f;
    [SerializeField] private float launchAcceleration = 20f;

    private Rigidbody2D rb;
    private float startY;
    private float targetY;

    public override void Enter()
    {
        base.Enter();

        Debug.Log("AI → Launch");

        rb = Context.Self.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError($"{name}: Rigidbody2D not found.");
            return;
        }

        // Character is assumed to already be on the ground.
        startY = Context.Self.transform.position.y;
        targetY = startY + targetHeight;

        // Launch upward.
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            launchSpeed
        );
    }

    public override void Play()
    {
        if (rb == null)
            return;

        // Accelerate toward launch speed.
        float newVerticalSpeed = Mathf.MoveTowards(
            rb.linearVelocity.y,
            launchSpeed,
            launchAcceleration * Time.deltaTime
        );

        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            newVerticalSpeed
        );

        // Reached the desired height.
        if (Context.Self.transform.position.y >= targetY)
        {
            Debug.Log("AI → Launch finished");

            RequestDecision();
        }
    }

    public override void Exit()
    {
        Debug.Log("AI → Exit Launch");

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );
        }
    }
}