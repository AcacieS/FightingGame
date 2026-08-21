using System;
using TMPro;
using UnityEngine;

[Serializable]
public class Match
{
    [SerializeField] private BattleParticipants battleParticipants;
    public Match(GameObject player, GameObject enemy)
    {
        battleParticipants = new BattleParticipants(player, enemy);
        UIManager.Instance.UpdateUI(player.GetComponent<Character>(), enemy.GetComponent<Character>());
    }
}