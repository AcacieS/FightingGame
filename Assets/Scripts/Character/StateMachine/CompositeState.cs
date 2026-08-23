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
            if (!childTransform.gameObject.activeSelf)
            continue;

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
        UtilityAction action = utility.ChooseAction(AI.Context);

        if (action == null)
        {
            Debug.LogError($"{name}: No UtilityAction was chosen!");
            return;
        }

        Debug.Log($"{name}: Chosen action = {action.name}");

        State nextState = action.GetState();

        if (nextState == null)
        {
            Debug.LogError(
                $"{name}: {action.name} returned NULL state!"
            );
            return;
        }

        Debug.Log($"{name}: Next state = {nextState.name}");

        ChangeChild(nextState);
    }

    protected void ChangeChild(State state)
    {
        currentChild?.Exit();

        currentChild = state;

        Debug.Log(
            $"{name}: Entering child {currentChild.name}"
        );

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