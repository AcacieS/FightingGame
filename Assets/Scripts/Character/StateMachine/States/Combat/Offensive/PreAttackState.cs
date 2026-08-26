using UnityEngine;

public class PreAttackState : State
{
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Pre Attack");
    }

    public override void Play()
    {
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestDecision();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("AI → Exit pre Attack");
    }
}