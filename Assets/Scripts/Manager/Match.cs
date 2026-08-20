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
    }
    public void AssignCharactersUI(TextMeshProUGUI playerUI, TextMeshProUGUI enemyUI)
    {
        battleParticipants.AssignCharactersUI(playerUI, enemyUI);
    }
    public void StartMatch()
    {
        battleParticipants.AssignHp();
    }
    public void FinishMatch()
    {
        
    }
}