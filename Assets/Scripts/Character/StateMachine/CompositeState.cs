using System.Collections.Generic;
using UnityEngine;

public class CompositeState : State
{
    [SerializeField]
    private List<State> states = new();

    [SerializeField]
    private List<UtilityAction> actions = new();

    private Utility utility;

    protected State currentChild;
    [ContextMenu("SetStates")]
    private void SetStates()
    {
        states = new List<State>();
        actions = new List<UtilityAction>();
        foreach (Transform childTransform in transform)
        {
            State state = childTransform.GetComponent<State>();
            
            if (state != null)
            {
                states.Add(state);
            }
            UtilityAction action = childTransform.GetComponent<UtilityAction>();
            
            if (action != null)
            {
                actions.Add(action);
            }
        }
    }
    public override void Initialize(CharacterController ai)
    {
        base.Initialize(ai);
        utility = new Utility(actions);

        foreach (State state in states)
        {
            state.Initialize(ai);
        }

        foreach (UtilityAction action in actions)
        {
            action.Initialize(ai);
        }
    }
    

    public void MakeDecision()
    {
        UtilityAction action = utility.ChooseAction(
            AI.Context
        );

        if (action == null)
            return;

        State nextState = action.GetState();

        ChangeChild(nextState);
    }

    protected void ChangeChild(State state)
    {
        currentChild?.Exit();

        currentChild = state;

        currentChild.Enter();
    }
    public override void Enter()
    {
        MakeDecision();
    }

    public override void Update()
    {
        currentChild?.Update();
    }

    public override void Exit()
    {
        currentChild?.Exit();
        currentChild = null;
    }
}