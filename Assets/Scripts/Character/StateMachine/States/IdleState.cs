using UnityEngine;

public class IdleState : State
{
    private float timer;
    [SerializeField] private float duration = 5;

    public override void Enter()
    {
        timer = 0f;
        //duration = Random.Range(1f, 3f);

        Debug.Log("AI → Idle");
        AI.Character.Move(0);
    }

    public override void Play()
    {
        AI.Character.Move(0);
        timer += Time.deltaTime;

        if (timer >= duration)
        {
            RequestRootDecision();
        }
    }
}