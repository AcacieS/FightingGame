using UnityEngine;

public class HurtState : ActionState
{
    [SerializeField] private SpriteRenderer characterHurt;
    [SerializeField] private Color hurtColor = Color.red;
    [SerializeField] private float duration = 0.2f;

    private Color originalColor;
    private float timer;

    public override void Enter()
    {
        base.Enter();

        if (characterHurt == null)
        {
            Debug.LogError($"{name}: Character Hurt SpriteRenderer is not assigned.", this);
            return;
        }

        originalColor = characterHurt.color;
        timer = 0f;

        characterHurt.color = hurtColor;
    }

    public override void Play()
    {
        if (characterHurt == null)
            return;

        timer += Time.deltaTime;

        if (timer >= duration)
        {
            characterHurt.color = originalColor;
            AI.StopReactionState();
        }
    }

    public override void Exit()
    {
        // Make sure the original color is restored
        // even if this state is interrupted early.
        if (characterHurt != null)
            characterHurt.color = originalColor;
    }
}