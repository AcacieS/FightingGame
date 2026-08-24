using UnityEngine;
using System;
[Serializable]
public class BattleParticipants
{
    [ReadOnly, SerializeField] private GameObject player;
    [ReadOnly, SerializeField] private GameObject enemy;
    public Character Player => player.GetComponent<Character>();
    public Enemy Enemy => enemy.GetComponent<Enemy>();
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