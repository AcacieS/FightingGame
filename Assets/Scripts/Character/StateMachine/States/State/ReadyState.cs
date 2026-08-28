using UnityEngine;

public class ReadyState : ActionState
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Ready");
    }

    public override void Play()
    {
        if (Context.Self.IsAnimFinished(animName))
        {
            AI.StopState();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("AI → Exit Ready");
    }
}