using UnityEngine;
using System;
[Serializable]
public class BattleParticipants
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy;
    [SerializeField] private bool hasPlayerFinishAnim = false;
    [SerializeField] private bool hasEnemyFinishAnim = false;
    public bool SetFinishAnim(Character character)
    {
        if(character == Player)
        {
            hasPlayerFinishAnim = true;
        }else if(character == Enemy)
        {
            hasEnemyFinishAnim = true;
        }
        return hasPlayerFinishAnim && hasEnemyFinishAnim;
    }
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