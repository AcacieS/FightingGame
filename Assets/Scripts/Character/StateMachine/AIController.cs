using UnityEngine;

public class AIController : MonoBehaviour
{
    private Character character;
    private Character target;

    [Header("AI")]
    [SerializeField] private State startingState;
    [Header("Reaction State")]
    private State reactionState;
    [SerializeField] private State readyState;
    [SerializeField] private State hurtState;
    [SerializeField] private State hurtInterruptState;
    [SerializeField] private State deadState;
    [SerializeField, Range(0f, 1f)]
    private float percentageBlockState;
    [SerializeField] private State blockState;
    [Header("Debug Values")]
    [ReadOnly, SerializeField] private State currentState;
    private Context context;

    public Character Character => character;
    public Character Target => target;
    public Context Context => context;

    private void Awake()
    {
        
    }
    
    public void StartReadyPhase()
    {
        Debug.Log("Should be in Ready State");
        ChangeState(readyState);
    }
    public void StartFightingPhase()
    {
        ChangeState(startingState);
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
        if (hurtState != null)
            hurtState.Initialize(this);
        if (deadState != null)
            deadState.Initialize(this);
        if(blockState!=null)
            blockState.Initialize(this);
        if(readyState!=null)
            readyState.Initialize(this);
        
    }
    
    private void Update()
    {
        //context.Update();
        currentState?.Play();
        reactionState?.Play();

    }
    public bool TryBlock()
    {
        if (blockState == null||currentState == blockState)
            return false;

        if (Random.value <= percentageBlockState)
        {
            ChangeState(blockState);
            return true;
        }

        return false;
    }
    private void HandleHurt(int hp, bool isInterruptible)
    {
        Debug.Log($"{name}: HandleHurt | HP = {hp}");

        // Death always has priority.
        if (hp <= 0)
        {
            ChangeState(deadState);
            return;
        }

        // If the current action can be interrupted,
        // replace it with the hurt reaction.
        if (isInterruptible)
        {
            ChangeState(hurtInterruptState);
        }
        else
        {
            // Current action cannot be interrupted.
            // Play the hurt reaction separately if desired.
            ChangeReactionState(hurtState);
        }
    }
    public void StopReactionState()
    {
        reactionState?.Exit();
        reactionState = null;

    }
    public void StopState()
    {
        currentState?.Exit();
        currentState = null;
    }
    public void RequestDecision()
    {
        ChangeState(startingState);
    }
    public void ChangeReactionState(State newState)
    {
        if (newState == null)
            return;

        reactionState?.Exit();

        reactionState = newState;

        reactionState.Enter();
    }
    public void ChangeState(State newState)
    {
        if (newState == null)
            return;

        currentState?.Exit();

        currentState = newState;
        context.SetCurrentState(currentState);

        currentState.Enter();
    }
    private void OnDestroy()
    {
        if (character != null)
            character.OnHurt -= HandleHurt;
    }
    private void OnDisable()
    {
        if (character != null)
        {
            character.OnHurt-=HandleHurt;
        }
    }
}