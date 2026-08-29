using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/Character/CharacterInfo")]
public class CharacterInfo : ScriptableObject
{
    [SerializeField] private string characterName;
    [SerializeField] private int hp;
    [SerializeField] private Sprite profileImg;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float accelerationSpeed;
    public int Hp => hp;
    public string Name => characterName;
    public Sprite ProfileImg => profileImg;
    public float MoveSpeed => moveSpeed;
    public float Acceleration => accelerationSpeed;
}
