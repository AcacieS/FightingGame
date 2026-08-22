using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected CharacterController AI { get; private set; }

    protected CompositeState Parent { get; private set; }

    public virtual void Initialize(CharacterController ai)
    {
        AI = ai;
        Parent = GetComponentInParent<CompositeState>();
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
    protected void RequestDecision()
    {
        Parent.MakeDecision();
    }
}