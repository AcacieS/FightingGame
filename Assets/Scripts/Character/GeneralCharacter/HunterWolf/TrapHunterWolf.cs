using UnityEngine;

public class TrapHunterWolf : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float lifeTime = 15f;

    [Header("Ground Detection")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Trap")]
    [SerializeField] private float stunDuration = 2f;
    [SerializeField] private LayerMask playerLayer;

    private Rigidbody2D rb;
    private TrapHunterPool pool;

    private Timer timer;

    private bool isFlying;
    private bool isActivated;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(TrapHunterPool pool)
    {
        this.pool = pool;
    }

    public void Launch(Vector2 velocity)
    {
        timer = new Timer(lifeTime);

        isFlying = true;
        isActivated = false;

        rb.linearVelocity = velocity;
    }

    private void FixedUpdate()
    {
        if (!isFlying)
            return;

        if (timer.IsOver())
        {
            ReturnToPool();
            return;
        }

        Vector2 velocity = rb.linearVelocity;

        if (velocity.magnitude <= 0f)
            return;

        float distance = velocity.magnitude * Time.fixedDeltaTime;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            velocity.normalized,
            distance,
            groundLayer
        );

        if (hit.collider != null)
        {
            rb.position = hit.point;

            ActivateTrap();
        }
    }

    private void ActivateTrap()
    {
        isFlying = false;
        isActivated = true;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActivated)
            return;

        if (((1 << other.gameObject.layer) & playerLayer) == 0)
            return;

        // Stun the player here

        ReturnToPool();
    }

    private void ReturnToPool()
    {
        isFlying = false;
        isActivated = false;

        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Dynamic;

        pool.ReturnTrap(this);
    }
}
