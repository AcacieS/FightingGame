using UnityEngine;

public class HurtState : State
{
    private readonly ReactionState reaction;

    private float timer;

    public override void Enter()
    {
        Debug.Log("AI → Hit");

        timer = 0f;
    }

    public override void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            RequestDecision();
            //AI.EnterCombat();
        }
    }
}