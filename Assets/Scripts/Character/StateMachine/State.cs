using UnityEngine;

public abstract class State : MonoBehaviour
{
    protected AIController AI { get; private set; }
    protected State Parent { get; private set; }
    protected Context Context => AI.Context;
    [SerializeField] protected string animName;

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
        //TODO: LOOK AT NOT ALWAYS WANTED
        Context.Self.LookAt(Context.Target);
        Context.SetCurrentState(this);
        Context.Self.PlayAnim(animName);
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
        else if (Parent is CompositeState composite)
        {
            Debug.Log($"{name} → Composite MakeDecision()");
            composite.MakeDecision();
        }
        else
        {
            Debug.Log($"{name} → Root Decision");
            RequestRootDecision();
        }
    }

    protected void RequestRootDecision()
    {
        AI.RequestDecision();
    }
}