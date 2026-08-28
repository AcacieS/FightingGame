using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Match currentMatch;
    public event System.Action<Match> OnMatchStart;
    
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
        StartMatch();
    }
    private void StartMatch()
    {
        if (currentMatch == null)
        {
            Debug.LogError("Please assigned Player and Enemy in GameManager");
            return;
        }
        OnMatchStart?.Invoke(currentMatch);  
    }

}
