using UnityEngine;

public class Bottle : MonoBehaviour
{
    private Character owner;
    private int damage;

    private LayerMask charactersLayer;
    private LayerMask groundLayer;

    private Rigidbody2D rb;

    [SerializeField] private float maxLifetime = 5f;

    private void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    public void Initialize(
        Character owner,
        int damage,
        Vector2 velocity,
        LayerMask charactersLayer,
        LayerMask groundLayer)
    {
        this.owner = owner;
        this.damage = damage;
        this.charactersLayer = charactersLayer;
        this.groundLayer = groundLayer;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                $"{name}: Bottle requires a Rigidbody2D.",
                this
            );

            return;
        }

        rb.linearVelocity = velocity;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // =========================
        // GROUND
        // =========================

        if (IsLayerInMask(other.gameObject.layer, groundLayer))
        {
            Debug.Log($"{name}: Bottle hit ground.");

            Destroy(gameObject);
            return;
        }

        // =========================
        // CHARACTER
        // =========================

        if (!IsLayerInMask(other.gameObject.layer, charactersLayer))
            return;

        Character character =
            other.GetComponentInParent<Character>();

        if (character == null)
            return;

        // Don't hurt the owner.
        if (character == owner)
            return;

        Debug.Log(
            $"{name}: Bottle hit {character.name}"
        );

        character.Hurt(damage);

        Destroy(gameObject);
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}