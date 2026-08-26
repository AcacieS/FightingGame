using UnityEngine;

public class IdleState : State
{
    private float timer;
    [SerializeField] private float minDuration = 5;
    [SerializeField] private float maxDuration = 5;
    private float duration;
    public override void Enter()
    {
        base.Enter();
        timer = 0f;
        duration = Random.Range(minDuration, maxDuration);
        Debug.Log("AI → Idle");
        AI.Character.Move(0);
    }

    public override void Play()
    {
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            RequestRootDecision();
        }
    }
}