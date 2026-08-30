using System.Collections;
using UnityEngine;

/// <summary>
/// Owns what happens between a character dying and the game being allowed to move on.
///
/// Drop it on any GameObject with a Character. It watches for death, holds for as long as
/// the death animation actually runs, and then reports Finished. Whoever wants to react to
/// the death - a scene change, a results screen, a fade - waits on that instead of guessing
/// a duration.
///
/// Deliberately reads Character.IsDead rather than hooking Die(): Character exposes no death
/// event, and polling one bool costs nothing but means this works on every character in the
/// project without editing a single one of their scripts.
///
/// The animation itself is still raised by Character.Die(), which sets the "Death" trigger.
/// This component does not duplicate that unless Raise Trigger is ticked.
/// </summary>
[RequireComponent(typeof(Character))]
[DisallowMultipleComponent]
public class DeathSequence : MonoBehaviour
{
    public enum DurationMode
    {
        /// <summary>Measure the death state on the Animator, so each character holds for its own clip.</summary>
        MeasureAnimation,

        /// <summary>Ignore the Animator and hold for Fixed Duration.</summary>
        Fixed
    }

    [Header("Animation")]
    [Tooltip("Name of the death STATE in the Animator, used to confirm the character really entered it before measuring. Must match the state, not the clip.")]
    [SerializeField] private string deathStateName = "Death";
    [Tooltip("Trigger that starts the death animation. Character.Die() already raises this, so leave Raise Trigger off unless a character overrides Die() without calling base.")]
    [SerializeField] private string deathTrigger = "Death";
    [Tooltip("Raise the trigger from here as well. Off by default - double-raising is harmless but hides a character that forgot to call base.Die().")]
    [SerializeField] private bool raiseTrigger;

    [Header("Duration")]
    [SerializeField] private DurationMode durationMode = DurationMode.MeasureAnimation;
    [Tooltip("Used when Duration Mode is Fixed.")]
    [SerializeField] private float fixedDuration = 2f;
    [Tooltip("Held after the clip ends, so the last frame reads before anything else happens.")]
    [SerializeField] private float extraHold = 0.5f;
    [Tooltip("Used when Measure Animation cannot find the death state - no Animator, no controller, or the trigger never landed.")]
    [SerializeField] private float fallbackDuration = 2f;
    [Tooltip("How long to wait for the Animator to reach the death state before giving up and using Fallback Duration.")]
    [SerializeField] private float stateEntryTimeout = 0.5f;

    [Header("On Death")]
    [Tooltip("Zero out horizontal velocity every step of the sequence, so the corpse does not slide off.")]
    [SerializeField] private bool freezeMovement = true;
    [Tooltip("Optional one-shot played the moment the character dies.")]
    [SerializeField] private Audio deathAudio;

    [Header("Debug")]
    [SerializeField] private bool logSequence;

    private Character character;
    private Animator animator;
    private Rigidbody2D body;

    /// <summary>True from the frame death is noticed onward.</summary>
    public bool HasDied { get; private set; }

    /// <summary>True once the death animation has played out and the hold has elapsed.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Raised once, when the sequence completes.</summary>
    public event System.Action Finished;

    private void Awake()
    {
        character = GetComponent<Character>();
        animator = GetComponent<Animator>();
        body = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (HasDied || character == null || !character.IsDead)
            return;

        HasDied = true;
        StartCoroutine(Run());
    }

    private void FixedUpdate()
    {
        // Held every step rather than zeroed once: a corpse still has colliders, so a shove
        // landing after death would otherwise send it drifting for the whole sequence.
        if (HasDied && !IsFinished && freezeMovement && body != null)
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
    }

    private IEnumerator Run()
    {
        if (logSequence)
            Debug.Log($"[DeathSequence] {name} died", this);

        PlayDeathAudio();

        if (raiseTrigger)
            RaiseTrigger();

        float hold = durationMode == DurationMode.Fixed
            ? fixedDuration
            : fallbackDuration;

        if (durationMode == DurationMode.MeasureAnimation)
            yield return MeasureThenHold(measured => hold = measured);

        if (logSequence)
            Debug.Log($"[DeathSequence] {name} holding {hold:0.00}s", this);

        if (hold > 0f)
            yield return new WaitForSeconds(hold);

        IsFinished = true;
        Finished?.Invoke();

        if (logSequence)
            Debug.Log($"[DeathSequence] {name} finished", this);
    }

    /// <summary>
    /// Waits for the Animator to actually reach the death state, then reports how long that
    /// state runs. Measured rather than configured so each character holds for its own clip,
    /// and so retiming the animation cannot silently desync the wait.
    /// </summary>
    private IEnumerator MeasureThenHold(System.Action<float> report)
    {
        if (animator == null || animator.runtimeAnimatorController == null)
        {
            report(fallbackDuration);
            yield break;
        }

        float deadline = Time.time + stateEntryTimeout;

        // The trigger is consumed on the next graph evaluation, so the death state is not
        // yet current on the frame Die() ran.
        while (Time.time < deadline)
        {
            AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);

            if (!animator.IsInTransition(0) && state.IsName(deathStateName))
            {
                // Divided by speed so a retimed state or a slowed Animator still lines up.
                float speed = Mathf.Abs(state.speed * animator.speed);
                float length = speed > 0.01f ? state.length / speed : state.length;

                report(length + extraHold);
                yield break;
            }

            yield return null;
        }

        Debug.LogWarning(
            $"{name}: never entered Animator state '{deathStateName}' within {stateEntryTimeout:0.##}s, " +
            $"so the death hold falls back to {fallbackDuration:0.##}s. Check the state name and the '{deathTrigger}' trigger.",
            this
        );

        report(fallbackDuration);
    }

    private void RaiseTrigger()
    {
        if (animator == null || string.IsNullOrEmpty(deathTrigger))
            return;

        // Checked against the declared parameters: SetTrigger on a name the controller does
        // not have logs a warning every call.
        foreach (AnimatorControllerParameter parameter in animator.parameters)
        {
            if (parameter.name != deathTrigger)
                continue;

            if (parameter.type == AnimatorControllerParameterType.Trigger)
                animator.SetTrigger(deathTrigger);

            return;
        }
    }

    private void PlayDeathAudio()
    {
        if (deathAudio == null)
            return;

        AudioEventChannel channel = AudioEventChannel.Instance;

        if (channel != null)
            channel.Play(deathAudio);
    }

    /// <summary>
    /// Yieldable wait for callers that want to hold until the death has played out:
    /// <c>yield return sequence.WaitForFinish();</c>
    /// </summary>
    public IEnumerator WaitForFinish()
    {
        while (!IsFinished)
            yield return null;
    }
}
