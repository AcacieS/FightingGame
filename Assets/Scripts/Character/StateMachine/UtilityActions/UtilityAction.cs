using System.Linq;
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

    protected AIController AI;
    [ReadOnly, SerializeField] private float _finalScore; 
    [SerializeField] private bool _activateTestScore;
    [SerializeField] private float _testScore;
    public void Awake()
    {
        //TODO: Might not need anymore state
        if (state == null)
        {
            Debug.Log("gets state");
            state = GetComponent<State>();
        }
        
        SetActions();
    }
    public void Start()
    {
        
    }

    [ContextMenu("SetActions")]
    private void SetActions()
    {
        considerations = GetComponents<Consideration>()
            .Where(c => c.enabled)
            .ToArray();
        requirements = GetComponents<Requirement>()
            .Where(r => r.enabled)
            .ToArray();
    }
    
    public virtual void Initialize(AIController ai)
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

        float score = 0f;

        if (considerations.Length > 0)
        {
            float totalScore = 0f;
            float totalWeight = 0f;

            foreach (Consideration consideration in considerations)
            {
                totalScore += consideration.Evaluate(context) * consideration.Weight;
                totalWeight += consideration.Weight;
            }

            if (totalWeight > 0f)
            {
                score = totalScore / totalWeight;
            }
        }

        score += state.Weight;

        _finalScore = score;
        if (_activateTestScore)
        {
            return _testScore;
        }
        return _finalScore;
    }
}