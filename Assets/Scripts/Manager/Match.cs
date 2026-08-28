using System;
using TMPro;
using UnityEngine;

[Serializable]
public class Match
{
    [ReadOnly, SerializeField] MatchState matchState; 
    [SerializeField] private BattleParticipants battleParticipants;
    public MatchState State => matchState;
    public event Action OnMatchIntro;
    public event Action OnMatchPreReady;
    public event Action OnMatchReady;
    public event Action OnMatchFighting;
    public event Action OnMatchFinished;
    public void FinishReadyAnimation(Character character)
    {
        if (battleParticipants.SetFinishAnim(character))
        {
            SetMatchState(MatchState.Ready);
        }
    }
    public void SetMatchState(MatchState newState)
    {
        matchState = newState;

        switch (newState)
        {
            case MatchState.Intro:
                OnMatchIntro?.Invoke();
                break;
            case MatchState.PreReady:
                OnMatchPreReady?.Invoke();
                break;
            case MatchState.Ready:
                OnMatchReady?.Invoke();
                break;

            case MatchState.Fighting:
                OnMatchFighting?.Invoke();
                break;

            case MatchState.Finished:
                OnMatchFinished?.Invoke();
                break;
        }
    }
    public Character Player => battleParticipants.Player;
    public Enemy Enemy => battleParticipants.Enemy;
    public Match(GameObject playerPrefab, Transform playerPos, GameObject enemyPrefab, Transform enemyPos)
    {
        GameObject player = UnityEngine.Object.Instantiate(playerPrefab, playerPos.position, Quaternion.identity);
        GameObject enemy = UnityEngine.Object.Instantiate(enemyPrefab, enemyPos.position, Quaternion.identity);

        UIManager.Instance.UpdateUI(player.GetComponent<Character>(), enemy.GetComponent<Character>());
        battleParticipants = new BattleParticipants(player, enemy);
    }
    public void EndMatch()
    {
        battleParticipants.DestroyParticipants();
    }
}