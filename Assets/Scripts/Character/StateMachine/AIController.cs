using UnityEngine;

public class AIController : MonoBehaviour
{
    private Character character;
    private Character target;

    [Header("AI")]
    [SerializeField] private State startingState;
    [Header("Reaction State")]
    
    [SerializeField] private State hurtState;
    [SerializeField] private State deadState;
    [Header("Debug Values")]
    [ReadOnly, SerializeField] private State currentState;
    private Context context;

    public Character Character => character;
    public Character Target => target;
    public Context Context => context;

    private void Awake()
    {
        
    }

    private void Start()
    {
        context = Context.Instance;
        character = context.Self;
        target = context.Target;
        if (character == null)
        {
            Debug.LogError("Character null");
        }
        character.OnHurt += HandleHurt;
        if (startingState == null)
        {
            Debug.LogError("No starting state assigned.", this);
            return;
        }

        startingState.Initialize(this);
        ChangeState(startingState);
    }
    private void OnDisable()
    {
        if (character != null)
        {
            character.OnHurt-=HandleHurt;
        }
        
    }

    private void Update()
    {
        //context.Update();
        currentState?.Play();
    }
    private void HandleHurt(int Hp)
    {
        if (Hp <= 0)
        {
            //TODO: DEATH
            ChangeState(hurtState);
        }
        else
        {
            //TODO: HURT
            ChangeState(deadState);
        }
        
    }
    public void RequestDecision()
    {
        ChangeState(startingState);
    }

    public void ChangeState(State newState)
    {
        if (newState == null)
            return;

        currentState?.Exit();

        currentState = newState;

        currentState.Enter();
    }
    private void OnDestroy()
    {
        if (character != null)
            character.OnHurt -= HandleHurt;
    }
}