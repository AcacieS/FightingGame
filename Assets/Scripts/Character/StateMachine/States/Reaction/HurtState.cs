using UnityEngine;

public class HurtState : State
{
    //TODO: This will interrupt currentState
    public override void Enter()
    {
        base.Enter();
        Debug.Log("AI → Hurt");
        if (Context == null)
        {
            Debug.Log("Context null");
        }
        if (Context.Self == null)
        {
            Debug.Log("Context Self null");
        }
        Context.Self.Move(0);
    }

    public override void Play()
    {
        if (Context.Self.IsAnimFinished(animName))
        {
            RequestRootDecision();
        }
    }
    
}