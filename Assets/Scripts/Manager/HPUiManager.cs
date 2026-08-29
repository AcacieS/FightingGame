using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Drives a two-layer health bar. The lower RawImage is the track that shows through;
/// the upper "cover" RawImage is resized to match remaining health, so damage reads as
/// the cover being compressed toward one edge.
///
/// Put this on the bar's root (or anywhere in the Canvas) and point Cover at the top
/// RawImage. Nothing needs to reference it back - it subscribes to Character.OnHpChanged.
///
/// One instance drives one bar. Add a second for the boss and give it that character.
/// </summary>
[DisallowMultipleComponent]
public class HPUiManager : MonoBehaviour
{
    public enum Side
    {
        Left,
        Right
    }

    public enum Mode
    {
        /// <summary>Crops the texture. The artwork keeps its proportions.</summary>
        Crop,

        /// <summary>Squashes the texture into the smaller rect.</summary>
        Stretch
    }

    [Header("Source")]
    [Tooltip("Leave empty to find it by tag at runtime - the player is spawned by Match, so it may not exist yet.")]
    [SerializeField] private Character character;
    [SerializeField] private string characterTag = "Player";
    [SerializeField] private float findInterval = 0.5f;

    [Header("Bar")]
    [Tooltip("The RawImage layered on top. Its width is driven to match remaining health.")]
    [SerializeField] private RawImage cover;
    [Tooltip("The edge that stays put while the bar shrinks.")]
    [SerializeField] private Side shrinkToward = Side.Left;
    [Tooltip("Crop keeps the artwork undistorted. Stretch squashes it into the smaller rect.")]
    [SerializeField] private Mode mode = Mode.Crop;
    [Tooltip("Off: the cover is the remaining health. On: the cover is the damage taken instead.")]
    [SerializeField] private bool coverShowsDamage;

    [Header("Animation")]
    [Tooltip("Seconds for the bar to travel its full length. 0 snaps instantly.")]
    [SerializeField] private float drainDuration = 0.25f;
    [Header("Debug")]
    [ReadOnly, SerializeField] private int currentHealth;
    [ReadOnly, SerializeField] private int maxHealth;

    private RectTransform coverRect;
    private float fullWidth;
    private float shownFraction = 1f;
    private float targetFraction = 1f;
    private float nextFindTime;
    private bool warnedNoCharacter;

    // Grace period before complaining that nothing was ever found to track.
    private const float FindWarnDelay = 3f;

    private void Awake()
    {
        if (cover == null)
        {
            Debug.LogError($"{name}: no Cover RawImage assigned, so the bar cannot move.", this);
            enabled = false;
            return;
        }

        coverRect = cover.rectTransform;
    }

    private void Start()
    {
        // Read in Start, not Awake: the layout has resolved by now, and this must be
        // captured before the first resize or we would measure an already-shrunk bar.
        fullWidth = coverRect.rect.width;

        if (fullWidth <= 0f)
            Debug.LogError($"{name}: the Cover RawImage has zero width. Give it a size before play.", this);

        ValidateAnchors();

        // Adopt an Inspector-assigned character explicitly; Resolve() skips when one is
        // already held, so without this it would never get subscribed.
        if (character != null)
            Bind(character);
        else
            Resolve();

        shownFraction = targetFraction;
        Apply(shownFraction);
    }

    private void OnEnable()
    {
        // Pairs with OnDisable. Bind is idempotent, so this cannot double-subscribe.
        if (character != null)
            Bind(character);
    }

    private void OnDisable() => Unsubscribe(character);

    private void Update()
    {
        if (character == null)
        {
            Resolve();
            WarnIfNeverFound();
        }

        if (Mathf.Approximately(shownFraction, targetFraction))
            return;

        shownFraction = drainDuration > 0f
            ? Mathf.MoveTowards(shownFraction, targetFraction, Time.deltaTime / drainDuration)
            : targetFraction;

        Apply(shownFraction);
    }

    /// <summary>Point the bar at a different character at runtime, e.g. on a new match.</summary>
    public void SetCharacter(Character next)
    {
        Bind(next);

        shownFraction = targetFraction;
        Apply(shownFraction);
    }

    /// <summary>
    /// Idempotent: unsubscribing first means calling this with the character already held
    /// still leaves exactly one subscription. That matters because a Character assigned in
    /// the Inspector has never been through SetCharacter and would otherwise never be
    /// subscribed at all.
    /// </summary>
    private void Bind(Character next)
    {
        Unsubscribe(character);

        character = next;

        Subscribe(character);

        if (character != null)
        {
            currentHealth = character.Hp;
            maxHealth = character.Info.Hp;
            targetFraction = FractionOf(character);
        }
        else
        {
            currentHealth = 0;
            maxHealth = 0;
            targetFraction = 1f;
        }
    }

    private void Resolve()
    {
        if (character != null || Time.time < nextFindTime)
            return;

        nextFindTime = Time.time + findInterval;

        if (string.IsNullOrEmpty(characterTag))
            return;

        GameObject tagged;

        try
        {
            tagged = GameObject.FindGameObjectWithTag(characterTag);
        }
        catch (UnityException)
        {
            Debug.LogError($"{name}: tag '{characterTag}' is not defined in this project.", this);
            characterTag = string.Empty;
            return;
        }

        if (tagged == null)
            return;

        Character found = tagged.GetComponentInParent<Character>();

        if (found == null)
            found = tagged.GetComponentInChildren<Character>();

        if (found != null)
            SetCharacter(found);
    }

    private void WarnIfNeverFound()
    {
        if (warnedNoCharacter || Time.time < FindWarnDelay)
            return;

        warnedNoCharacter = true;

        Debug.LogWarning(
            $"{name}: no Character tagged '{characterTag}' found after {FindWarnDelay:0} seconds, " +
            "so the bar has nothing to track.",
            this
        );
    }

    private void Subscribe(Character target)
    {
        if (target != null)
            target.OnHpChanged += HandleHpChanged;
    }

    private void Unsubscribe(Character target)
    {
        if (target != null)
            target.OnHpChanged -= HandleHpChanged;
    }

    private void HandleHpChanged(int hp)
    {
        currentHealth = hp;
        maxHealth = character.Info.Hp;

        targetFraction = FractionOf(character);
    }

    private static float FractionOf(Character target)
    {
        if (target == null || target.Info == null || target.Info.Hp <= 0)
            return 1f;

        return Mathf.Clamp01((float)target.Hp / target.Info.Hp);
    }

    private void Apply(float fraction)
    {
        if (fullWidth <= 0f)
            return;

        float width = coverShowsDamage ? 1f - fraction : fraction;

        coverRect.sizeDelta = new Vector2(
            fullWidth * width,
            coverRect.sizeDelta.y
        );

        // Crop mode has to slide the UV window along with the rect, or the texture would
        // squash instead of being revealed.
        cover.uvRect = mode == Mode.Crop
            ? new Rect(shrinkToward == Side.Left ? 0f : 1f - width, 0f, width, 1f)
            : new Rect(0f, 0f, 1f, 1f);
    }

    private void ValidateAnchors()
    {
        // A horizontally stretched RectTransform treats sizeDelta.x as a margin, not a
        // width, so driving it would move the edges instead of resizing the bar.
        if (!Mathf.Approximately(coverRect.anchorMin.x, coverRect.anchorMax.x))
        {
            Debug.LogError(
                $"{name}: the Cover RawImage is stretched horizontally. Set its anchor to a " +
                $"single {shrinkToward} edge so its width can be driven.",
                this
            );
            return;
        }

        float wanted = shrinkToward == Side.Left ? 0f : 1f;

        if (!Mathf.Approximately(coverRect.pivot.x, wanted))
        {
            Debug.LogWarning(
                $"{name}: Cover pivot X is {coverRect.pivot.x:0.##} but Shrink Toward is " +
                $"{shrinkToward}, so the bar will shrink from the wrong edge. Set pivot X to {wanted:0}.",
                this
            );
        }
    }
}
