using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AIController AI { get; private set; }
    protected State Parent { get; private set; }
    protected Context Context => AI.Context;
    [SerializeField, Range(0f, 1f)]
    private float stateWeight = 0f;
    public float Weight => stateWeight;

    public virtual void Initialize(AIController ai)
    {
        AI = ai;
        Transform parentTransform = transform.parent;
        if (parentTransform != null)
        {
            Parent = parentTransform.GetComponent<State>();
        }
    }

    public virtual void Enter()
    {
        Context.SetCurrentState(this);
    }
    public virtual void Play() { }
    public virtual void Exit() { }
    protected void RequestDecision()
    {
        Debug.Log(
            $"{name} RequestDecision | " +
            $"Parent = {(Parent == null ? "NULL" : Parent.name)} | " +
            $"Parent Type = {(Parent == null ? "NULL" : Parent.GetType().Name)}"
        );

        if (Parent is SequenceState sequence)
        {
            Debug.Log($"{name} → Sequence ChildFinished()");
            sequence.ChildFinished();
        }
        else
        {
            Debug.Log($"{name} → Root Decision");
            RequestRootDecision();
        }
    }
    protected void RequestParentCompositeDecision()
    {
        if (Parent is CompositeState composite)
        {
            Debug.Log($"{name} → Composite MakeDecision()");
            composite.MakeDecision();
        }
        else
        {
            RequestRootDecision();
        }
    }

    protected void RequestRootDecision()
    {
        AI.RequestDecision();
    }
}