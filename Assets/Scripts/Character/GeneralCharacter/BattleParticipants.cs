using UnityEngine;
using System;
using TMPro;
[Serializable]
public class BattleParticipants
{
    [SerializeField] private CharacterUI player;
    [SerializeField] private CharacterUI enemy;
    public BattleParticipants(GameObject playerPrefab, GameObject enemyPrefab)
    {
        player = new CharacterUI(playerPrefab);
        enemy = new CharacterUI(enemyPrefab);
    }
    public BattleParticipants(CharacterUI player, CharacterUI enemy)
    {
        this.player = player;
        this.enemy = enemy;
    }
    public void AssignCharactersUI(TextMeshProUGUI playerUI, TextMeshProUGUI enemyUI)
    {
        player.AssignCharacterUI(playerUI);
        enemy.AssignCharacterUI(enemyUI);
    }
    public void AssignHp()
    {
        player.AssignCharacterHP();
        enemy.AssignCharacterHP();
    }
    
}