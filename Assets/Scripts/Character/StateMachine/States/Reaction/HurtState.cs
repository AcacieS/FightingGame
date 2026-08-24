using UnityEngine;

public class HurtState : State
{
    private float timer;

    public override void Enter()
    {
        Debug.Log("AI → Hit");
        AI.Character.Move(0);
        timer = 0f;
    }

    public override void Play()
    {
        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            RequestRootDecision();
            //AI.EnterCombat();
        }
    }
}