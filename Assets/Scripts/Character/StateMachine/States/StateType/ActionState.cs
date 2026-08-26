using UnityEngine;

public abstract class ActionState : State
{
    [Header("ActionState")]

    [SerializeField]
    protected string animName;
    public override void Enter()
    {
        base.Enter();
        //TODO: CHANGE LOOKAT
        Context.Self.LookAt(Context.Target);

        if (!string.IsNullOrEmpty(animName))
        {
            Context.Self.PlayAnim(animName);
        }
    }
}