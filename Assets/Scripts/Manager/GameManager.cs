using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Match currentMatch;
    public Match Match => currentMatch;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    
    private void Start()
    {
        currentMatch.SetMatchState(MatchState.Intro);
    }

}
