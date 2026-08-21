using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Participants participants;
    [ReadOnly, SerializeField] private Match currentMatch;
    [ReadOnly] private int indexMatch = -1;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (participants == null)
        {
            Debug.LogError("participants null please assign");
        }
    }
    
    private void Start()
    {
        StartNewMatch();
    }
    
    [ContextMenu("Start New Match")]
    public void StartNewMatch()
    {
        if (indexMatch >= participants.EnemiesCount)
        {
            Debug.LogError("Too Much Matches"); 
            return;
        }

        if (currentMatch != null && indexMatch!=-1)
        {
            Debug.Log("end match: "+currentMatch);
            //currentMatch.EndMatch();
        }
        indexMatch++;
        currentMatch = new Match(participants.Player, participants.Enemy(indexMatch));
    }
}
