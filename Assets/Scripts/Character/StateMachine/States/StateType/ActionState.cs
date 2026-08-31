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

        Context.Self.LookAt(Context.Target, true);

        animator = Context.Self.GetComponent<Animator>();

        if (animator == null)
        {
            Debug.LogError($"{name}: Animator not found.");
            return;
        }

        originalAnimatorSpeed = animator.speed;

        if (!string.IsNullOrEmpty(animName))
        {
            if (overrideDuration)
            {
                Context.Self.PlayAnim(animName, desiredDuration);
            }
            else
            {
                Context.Self.PlayAnim(animName);
            }
            
        }
        Context.Self.transform.rotation =
            Quaternion.identity;
    }
    

    public override void Exit()
    {
        if (animator != null)
        {
            animator.speed = originalAnimatorSpeed;
        }
    }
}