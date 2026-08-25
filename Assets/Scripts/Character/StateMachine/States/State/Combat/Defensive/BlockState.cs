using UnityEngine;

public class BlockState : ActionState
{
    public override void Enter()
    {
        base.Enter();
        Context.Self.Move(0);
        Debug.Log("AI → Block");
    }

    public override void Play()
    {
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestRootDecision();
        }
    }
    public override void Exit()
    {
        Debug.Log("AI → Exit Block");
    }
}