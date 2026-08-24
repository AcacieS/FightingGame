using UnityEngine;

public class BlockState : State
{

    private float timer;
    public override void Enter()
    {
        Debug.Log("AI → Block");

        timer = 0f;

        // AI.Character.Block();
    }

    public override void Play()
    {
        timer += Time.deltaTime;

        if (timer >= 0.75f)
        {
            RequestRootDecision();
        }
    }
}