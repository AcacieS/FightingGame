using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    [SerializeField] private Participants participants;
    [SerializeField] private TextMeshProUGUI hpLeft;
    [SerializeField] private TextMeshProUGUI hpRight;
    [ReadOnly, SerializeField] List<Match> matches = new List<Match>();
    [ReadOnly, SerializeField] private Match currentMatch;
    
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
    private int indexMatch = 0;
    private void Start()
    {
        foreach (GameObject enemy in participants.Enemies)
        {
            Match newMatch = new Match(participants.Player, enemy);
            newMatch.AssignCharactersUI(hpLeft, hpRight);
            matches.Add(newMatch);
        }
        StartNewMatch();
    }
    [ContextMenu("Start New Match")]
    public void StartNewMatch()
    {
        if (indexMatch >= matches.Count)
        {
            Debug.LogError("Too Much Matches");
            return;
        }
        currentMatch = matches[indexMatch];
        currentMatch.StartMatch();
        indexMatch++;
    }
}
