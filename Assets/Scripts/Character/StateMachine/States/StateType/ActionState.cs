using UnityEngine;

public abstract class ActionState : State
{
    [Header("ActionState")]
    [SerializeField]
    protected string animName;

    [SerializeField]
    private bool overrideDuration = false;

    [SerializeField]
    [Min(0.01f)]
    private float desiredDuration = 1f;

    private Animator animator;
    private float originalAnimatorSpeed = 1f;

    public override void Enter()
    {
        base.Enter();

        Context.Self.LookAt(Context.Target);

        animator = Context.Self.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"{name}: Animator not found.");
            return;
        }

        originalAnimatorSpeed = animator.speed;

        if (!string.IsNullOrEmpty(animName))
        {
            SetAnimationSpeed();
            Context.Self.PlayAnim(animName);
        }
    }

    private void SetAnimationSpeed()
    {
        if (!overrideDuration)
            return;

        RuntimeAnimatorController controller =
            animator.runtimeAnimatorController;

        if (controller == null)
            return;

        foreach (AnimationClip clip in controller.animationClips)
        {
            if (clip.name != animName)
                continue;

            float originalDuration = clip.length;

            if (originalDuration <= 0f)
                return;

            animator.speed =
                originalDuration / desiredDuration;

            Debug.Log(
                $"{name}: {animName} | " +
                $"Original: {originalDuration:F2}s | " +
                $"Desired: {desiredDuration:F2}s | " +
                $"Speed: {animator.speed:F2}x"
            );

            return;
        }

        Debug.LogWarning(
            $"{name}: Could not find animation clip '{animName}'."
        );
    }
    

    public override void Exit()
    {
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
        }
    }
}