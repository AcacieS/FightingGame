using System;
using TMPro;
using UnityEngine;

[Serializable]
public class Match
{
    [ReadOnly, SerializeField] MatchState matchState; 
    [SerializeField] private BattleParticipants battleParticipants;
    public MatchState State => matchState;
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