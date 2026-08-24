using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AIController AI { get; private set; }
    protected CompositeState Parent { get; private set; }
    protected Context Context => AI.Context;
    public virtual void Initialize(AIController ai)
    {
        AI = ai;
        Parent = GetComponentInParent<CompositeState>();
    }

    public virtual void Enter() { }
    public virtual void Play() { }
    public virtual void Exit() { }
    protected void RequestDecision()
    {
        Parent?.MakeDecision();
    }

    protected void RequestRootDecision()
    {
        AI.RequestDecision();
    }
}