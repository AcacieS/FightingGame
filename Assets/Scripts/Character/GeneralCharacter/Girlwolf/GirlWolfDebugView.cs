using UnityEngine;

/// <summary>
/// TEST SCAFFOLDING - not part of the game.
///
/// Stands in for the animator until the clips exist: tints the wolf's sprite per state
/// and logs every transition, so you can see which state fired and why.
///
/// To remove it for a real build: delete this component off the prefab and delete this
/// file. Nothing in GirlWolf references it - it only listens to public events.
/// </summary>
[RequireComponent(typeof(GirlWolf))]
public class GirlWolfDebugView : MonoBehaviour
{
    [Header("Sprite")]
    [Tooltip("Left empty, the first SpriteRenderer in the children is used.")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("State Colours")]
    [SerializeField] private Color wandering = new Color(0.20f, 0.40f, 1.00f);   // blue
    [SerializeField] private Color chasing = new Color(0.20f, 0.80f, 0.30f);    // green
    [SerializeField] private Color bite = new Color(0.45f, 0.05f, 0.05f);       // dark red
    [SerializeField] private Color scratch = new Color(0.10f, 0.90f, 0.90f);    // cyan
    [SerializeField] private Color pounce = new Color(1.00f, 0.55f, 0.10f);     // orange
    [SerializeField] private Color accumulate = new Color(0.70f, 0.20f, 0.90f); // purple
    [SerializeField] private Color dead = new Color(0.35f, 0.35f, 0.35f);       // grey

    [Header("Logging")]
    [SerializeField] private bool logMoves = true;
    [SerializeField] private bool logTarget = true;
    [SerializeField] private bool logHealth = true;

    private GirlWolf wolf;

    private void Awake()
    {
        wolf = GetComponent<GirlWolf>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
            Debug.LogWarning($"{name}: no SpriteRenderer found, so state colours are off.", this);
    }

    private void OnEnable()
    {
        wolf.OnMoveChanged += HandleMoveChanged;
        wolf.OnMoveFinished += HandleMoveFinished;
        wolf.OnTargetChanged += HandleTargetChanged;
        wolf.OnHpChanged += HandleHpChanged;

        Tint(wolf.CurrentMove);
    }

    private void OnDisable()
    {
        wolf.OnMoveChanged -= HandleMoveChanged;
        wolf.OnMoveFinished -= HandleMoveFinished;
        wolf.OnTargetChanged -= HandleTargetChanged;
        wolf.OnHpChanged -= HandleHpChanged;
    }

    private void HandleMoveChanged(GirlWolf.WolfMove move)
    {
        Tint(move);

        if (logMoves)
        {
            Debug.Log(
                $"[GirlWolf] -> {move}  (hp {wolf.Hp}, dist {DistanceLabel()}, " +
                $"grounded {wolf.IsGrounded}, targetAlive {wolf.TargetIsAlive})",
                this
            );
        }
    }

    private void HandleMoveFinished(GirlWolf.WolfMove move)
    {
        if (logMoves)
            Debug.Log($"[GirlWolf] {move} finished", this);
    }

    private void HandleTargetChanged(Character next)
    {
        if (!logTarget)
            return;

        if (next != null)
            Debug.Log($"[GirlWolf] target acquired: {next.name}", this);
        else
            Debug.Log("[GirlWolf] target lost", this);
    }

    private void HandleHpChanged(int hp)
    {
        if (logHealth)
            Debug.Log($"[GirlWolf] hp {hp} ({wolf.HealthFraction:P0})", this);

        // Character fires OnHpChanged before calling Die(), so IsDead is still false here.
        // Paint it directly rather than going through Tint.
        if (hp <= 0 && spriteRenderer != null)
            spriteRenderer.color = dead;
    }

    private void Tint(GirlWolf.WolfMove move)
    {
        if (spriteRenderer == null)
            return;

        spriteRenderer.color = wolf.IsDead ? dead : ColourFor(move);
    }

    private Color ColourFor(GirlWolf.WolfMove move) => move switch
    {
        GirlWolf.WolfMove.Wandering => wandering,
        GirlWolf.WolfMove.Chasing => chasing,
        GirlWolf.WolfMove.Bite => bite,
        GirlWolf.WolfMove.Scratch => scratch,
        GirlWolf.WolfMove.Pounce => pounce,
        GirlWolf.WolfMove.Accumulate => accumulate,
        _ => Color.white
    };

    private string DistanceLabel()
    {
        Character current = wolf.Target;

        if (current == null)
            return "n/a";

        return Vector2.Distance(
            transform.position,
            current.transform.position
        ).ToString("F2");
    }
}
