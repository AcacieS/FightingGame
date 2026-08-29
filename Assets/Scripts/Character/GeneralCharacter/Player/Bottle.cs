using UnityEngine;

public class Bottle : MonoBehaviour
{
    private Character owner;
    private int damage;
    private LayerMask charactersLayer;

    private Rigidbody2D rb;

    public void Initialize(
        Character owner,
        int damage,
        Vector2 velocity,
        LayerMask charactersLayer)
    {
        this.owner = owner;
        this.damage = damage;
        this.charactersLayer = charactersLayer;

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
        // Only react to character layers.
        if ((charactersLayer.value &
             (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        Character character =
            other.GetComponentInParent<Character>();

        if (character == null)
            return;

        // Don't hurt the person throwing the bottle.
        if (character == owner)
            return;

        Debug.Log(
            $"{name}: Bottle hit {character.name}"
        );

        character.Hurt(
            damage
        );

        Destroy(gameObject);
    }
}