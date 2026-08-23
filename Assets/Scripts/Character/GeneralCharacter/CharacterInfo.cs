using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/Character/CharacterInfo")]
public class CharacterInfo : ScriptableObject
{
    [SerializeField] private int hp;
    [SerializeField] private Sprite profileImg;
    [SerializeField] private float moveSpeed;
    public int Hp => hp;
    public Sprite ProfileImg => profileImg;
    public float MoveSpeed => moveSpeed;
}
