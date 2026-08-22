using UnityEngine;

public class DeadState : State
{
    public override void Enter()
    {
        Debug.Log("AI → Dead");

        // AI.Character.Die();
    }

    public override void Update()
    {
        // Do nothing.
    }
}