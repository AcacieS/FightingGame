using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AIController AI { get; private set; }
    protected CompositeState Parent { get; private set; }
    protected Context Context => AI.Context;
    [SerializeField] protected string animName;

    public virtual void Initialize(AIController ai)
    {
        AI = ai;
        Parent = GetComponentInParent<CompositeState>();
    }

    public virtual void Enter()
    {
        Context.Self.LookAt(Context.Target);
        Context.SetCurrentState(this);
        Context.Self.PlayAnim(animName);
    }
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