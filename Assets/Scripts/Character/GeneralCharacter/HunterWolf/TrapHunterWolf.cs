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
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Sprite spriteGround;

    private Rigidbody2D rb;
    private TrapHunterPool pool;
    private HunterWolf owner;

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

        transform.rotation = Quaternion.Euler(new Vector3(0, 0, transform.rotation.z + .1f));

        if (hit.collider != null)
        {
            rb.position = hit.point;

            transform.rotation = Quaternion.Euler(new Vector3(0, 0, 0));
            sr.sprite = spriteGround;

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

        if (other.CompareTag("Player"))
        {
            Character character = other.GetComponent<Character>();

            if (character != null)
            {
                owner.Hit(character, 10, false);
            }
        }

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
