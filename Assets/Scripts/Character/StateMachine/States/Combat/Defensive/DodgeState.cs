using UnityEngine;

public class DodgeState : State
{

    private float timer;

    public override void Enter()
    {
        Debug.Log("AI → Dodge");

        timer = 0f;

        // AI.Character.Dodge();
    }

    public override void Play()
    {
        timer += Time.deltaTime;

        if (timer >= 0.5f)
        {
            RequestRootDecision();
        }
    }
}