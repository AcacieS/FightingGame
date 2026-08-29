using UnityEngine;

public class DeadState : ActionState
{
    public override void Enter()
    {

        Debug.Log("AI → Dead");
        Context.Self.Move(0);
        // AI.Character.Die();
    }

    public override void Play()
    {
        // Do nothing.
    }
}