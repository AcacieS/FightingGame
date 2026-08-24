using UnityEngine;

public class AttackState : State
{

    private float timer;

    public override void Enter()
    {
        Debug.Log("AI → Attack");

        timer = 0f;

        // AI.Character.Attack();
    }

    public override void Play()
    {
        timer += Time.deltaTime;

        if (timer >= 1f)
        {
            RequestRootDecision();
        }
    }
    
    public override void Exit()
    {
        Debug.Log("AI → Exit Attack");
    }
}