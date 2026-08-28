using UnityEngine;

public class WhileSequenceState : SequenceState
{
    [Header("Loop")]
    [SerializeField] private int maxLoops = 3;

    [ReadOnly, SerializeField]
    private int currentLoop;

    [ReadOnly, SerializeField]
    private bool failed;

    public override void Enter()
    {
        currentLoop = 0;
        failed = false;

        base.Enter();
    }

    public override void ChildFinished()
    {
        Debug.Log(
            $"{name}: While ChildFinished | " +
            $"currentChild={(currentChild ? currentChild.name : "NULL")} | " +
            $"index={currentIndex}"
        );

        if (currentChild is AttackState attackState)
        {
            AttackResult result = attackState.AttackResult;

            Debug.Log(
                $"{name}: Attack result = {result}"
            );

            if (result == AttackResult.Miss ||
                result == AttackResult.None)
            {
                failed = true;
            }
        }

        if (failed)
        {
            Debug.Log($"{name}: LOOP FAILED");

            currentChild?.Exit();
            currentChild = null;

            RequestDecision();
            return;
        }

        Debug.Log($"{name}: Calling base.ChildFinished()");

        base.ChildFinished();
    }

    protected override void SequenceFinish()
    {
        currentLoop++;
        Debug.Log($"currentLoop: "+currentLoop);
        if (currentLoop >= maxLoops)
        {
            RequestDecision();
            return;
        }

        RestartSequence();
    }
}