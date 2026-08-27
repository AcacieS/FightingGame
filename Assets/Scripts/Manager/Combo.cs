using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ComboTry
{
    [SerializeField] private string name;
    [SerializeField] private List<State> states = new();

    [ReadOnly, SerializeField]
    private int stateIndex;

    public string Name => name;

    public bool CanContinueFrom(State state)
    {
        return stateIndex < states.Count &&
            states[stateIndex] == state;
    }

    public State GetNextState()
    {
        if (stateIndex >= states.Count)
            return null;

        return states[stateIndex++];
    }

    public void Reset()
    {
        stateIndex = 0;
    }
    public float GetProgressBonus()
    {
        if (stateIndex <= 0)
            return 0f;

        return 0.2f * stateIndex;
    }
    public float GetProgress()
    {
        if (states.Count == 0)
            return 0f;

        return (float)stateIndex / states.Count;
    }

    public bool IsFinished =>
        stateIndex >= states.Count;
}