using UnityEngine;

public class BulletHunterWolf : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private LayerMask collisionLayer;

    private Rigidbody2D rb;
    private BulletHunterPool pool;
    private HunterWolf owner;

    private Timer timer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        timer = new Timer(lifeTime);
    }

    public void Initialize(BulletHunterPool pool, HunterWolf owner)
    {
        this.pool = pool;
        this.owner = owner;
    }

    public void Launch(Vector2 direction)
    {
        timer = new Timer(lifeTime);

        rb.linearVelocity = direction.normalized * speed;
    }

    private void FixedUpdate()
    {
        if (timer.IsOver())
        {
            ReturnToPool();
            return;
        }

        Vector2 velocity = rb.linearVelocity;
        float distance = velocity.magnitude * Time.fixedDeltaTime;

        if (distance <= 0f)
            return;

        RaycastHit2D hit = Physics2D.Raycast(
            rb.position,
            velocity.normalized,
            distance,
            collisionLayer
        );

        if (hit.collider != null)
        {
            rb.position = hit.point;

            if (hit.collider.CompareTag("Player"))
            {
                Character character = hit.collider.GetComponent<Character>();

                if (character != null)
                {
                    owner.Hit(character, 10, false);
                    ReturnToPool();
                }
            }

            if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                ReturnToPool();
            }
        }
    }

    private void ReturnToPool()
    {
        rb.linearVelocity = Vector2.zero;

        pool.ReturnBullet(this);
    }
}
