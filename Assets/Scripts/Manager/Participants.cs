using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Participants
{
    [SerializeField] private GameObject player;
    [SerializeField] private List<GameObject> enemies = new List<GameObject>();
    public GameObject Player => player;
    public List<GameObject> Enemies => new List<GameObject>(enemies);
    public int EnemiesCount => enemies.Count;
    public GameObject Enemy(int index) => enemies[index];
}