using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected CharacterControllerA AI { get; private set; }

    protected CompositeState Parent { get; private set; }

    public virtual void Initialize(CharacterControllerA ai)
    {
        AI = ai;
        Parent = GetComponentInParent<CompositeState>();
    }

    public virtual void Enter() { }
    public virtual void Tick() { }
    public virtual void Exit() { }
    protected void RequestDecision()
    {
        Parent.MakeDecision();
    }
}