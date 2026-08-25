using System.Collections.Generic;
using UnityEngine;

public class SequenceState : State
{
    [SerializeField]
    private List<State> states = new();

    [ReadOnly, SerializeField]
    private State currentChild;

    [ReadOnly, SerializeField]
    private int currentIndex = -1;

    public override void Initialize(AIController ai)
    {
        base.Initialize(ai);

        SetStates();

        foreach (State state in states)
        {
            state.Initialize(ai);
        }
    }

    [ContextMenu("SetStates")]
    private void SetStates()
    {
        states = new List<State>();

        foreach (Transform childTransform in transform)
        {
            if (!childTransform.gameObject.activeSelf)
                continue;

            State state = childTransform.GetComponent<State>();

            if (state != null)
            {
                states.Add(state);
            }
        }
    }

    public override void Enter()
    {
        currentIndex = -1;
        PlayNextState();
    }

    public override void Play()
    {
        currentChild?.Play();
    }

    private void PlayNextState()
    {
        // Exit previous child
        currentChild?.Exit();

        currentIndex++;

        // Sequence finished
        if (currentIndex >= states.Count)
        {
            currentChild = null;
            currentIndex = -1;

            RequestRootDecision();
            return;
        }

        currentChild = states[currentIndex];

        Debug.Log(
            $"{name}: Sequence → {currentChild.name}"
        );

        currentChild.Enter();
    }

    public void ChildFinished()
    {
        PlayNextState();
    }

    public override void Exit()
    {
        currentChild?.Exit();

        currentChild = null;
        currentIndex = -1;
    }
}