using UnityEngine;
using System;
using TMPro;
[Serializable]
public class BattleParticipants
{
    [ReadOnly, SerializeField] private GameObject player;
    [ReadOnly, SerializeField] private GameObject enemy;
    public BattleParticipants(GameObject playerPrefab, GameObject enemyPrefab)
    {
        player = playerPrefab;
        enemy = enemyPrefab;
    }
    
}