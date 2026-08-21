using UnityEngine;
using System;
using TMPro;
[Serializable]
public class BattleParticipants
{
    [ReadOnly, SerializeField] private GameObject player;
    [ReadOnly, SerializeField] private GameObject enemy;
    public BattleParticipants(GameObject player, GameObject enemy)
    {
        this.player = player;
        this.enemy = enemy;
    }
    public void DestroyParticipants()
    {
        UnityEngine.Object.Destroy(player);
        UnityEngine.Object.Destroy(enemy);
    }
    
}