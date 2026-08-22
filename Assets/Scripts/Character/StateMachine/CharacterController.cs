using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Characters")]
    [SerializeField] private Character character;
    [SerializeField] private Character target;

    [Header("AI")]
    [SerializeField] private State startingState;
    [SerializeField] private State reactionState;
    private State currentState;
    private Context context;

    public Character Character => character;
    public Character Target => target;
    public Context Context => context;

    private void Awake()
    {
        context = new Context(character, target);
        character.OnHurt += HandleHurt;
        if (startingState == null)
        {
            Debug.LogError("No starting state assigned.", this);
            return;
        }

        startingState.Initialize(this);
        reactionState.Initialize(this);
    }

    private void Start()
    {
        ChangeState(startingState);
    }

    private void Update()
    {
        context.Update();
        currentState?.Update();
    }
    private void HandleHurt(int Hp)
    {
        if (Hp <= 0)
        {
            //TODO: DEATH
            ChangeState(reactionState);
        }
        else
        {
            //TODO: HURT
            ChangeState(reactionState);
        }
        
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