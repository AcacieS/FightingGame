using UnityEngine;

public class UtilityAction : MonoBehaviour
{
    [Header("Requirements")]
    [ReadOnly, SerializeField]
    private Requirement[] requirements;

    [Header("Considerations")]
    [ReadOnly, SerializeField] private Consideration[] considerations;
    
    [SerializeField]
    [Tooltip("The State that this Action is for, auto-assign State on same GameObject if null")]
    private State state;

    protected CharacterController AI;
    public void Start()
    {
        //TODO: Might not need anymore state
        if (state != null)
        {
            state = GetComponent<State>();
        }
        
        SetActions();
    }

    [ContextMenu("SetActions")]
    private void SetActions()
    {
        considerations = GetComponents<Consideration>();
        requirements = GetComponents<Requirement>();
    }
    
    public virtual void Initialize(CharacterController ai)
    {
        AI = ai;
    }

    public State GetState()
    {
        return state;
    }

    public bool CanExecute(Context context)
    {
        foreach (Requirement requirement in requirements)
        {
            if (!requirement.IsMet(context))
                return false;
        }

        return true;
    }

    public float CalculateScore(Context context)
    {
        if (!CanExecute(context))
            return 0f;

        if (considerations.Length == 0)
            return 0f;

        float totalScore = 0f;
        float totalWeight = 0f;

        foreach (Consideration consideration in considerations)
        {
            totalScore += consideration.Evaluate(context);
            totalWeight += consideration.Weight;
        }

        if (totalWeight <= 0f)
            return 0f;

        return totalScore / totalWeight;
    }
}