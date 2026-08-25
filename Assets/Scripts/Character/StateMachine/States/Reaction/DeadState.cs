using UnityEngine;

public class DeadState : ActionState
{
    public override void Enter()
    {
        if (Context == null)
        {
            Debug.Log("Context null");
        }
        Debug.Log($"Context: {Context}");
        Debug.Log($"Context.Self: {Context?.Self}");

        Debug.Log("AI → Dead");
        Context.Self.Move(0);
        // AI.Character.Die();
    }

    public override void Play()
    {
        // Do nothing.
    }
}