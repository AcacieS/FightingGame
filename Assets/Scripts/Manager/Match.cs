using System;
using TMPro;
using UnityEngine;

[Serializable]
public class Match
{
    [SerializeField] private BattleParticipants battleParticipants;
    public Match(GameObject playerPrefab, Transform playerPos, GameObject enemyPrefab, Transform enemyPos)
    {
        GameObject player = UnityEngine.Object.Instantiate(playerPrefab, playerPos);
        GameObject enemy = UnityEngine.Object.Instantiate(enemyPrefab, enemyPos);

        UIManager.Instance.UpdateUI(player.GetComponent<Character>(), enemy.GetComponent<Character>());
        battleParticipants = new BattleParticipants(player, enemy);
    }
    public void EndMatch()
    {
        battleParticipants.DestroyParticipants();
    }
}