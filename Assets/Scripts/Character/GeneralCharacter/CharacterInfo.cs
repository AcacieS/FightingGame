using UnityEngine;

[CreateAssetMenu(fileName = "CharacterInfo", menuName = "Scriptable Objects/Character/CharacterInfo")]
public class CharacterInfo : ScriptableObject
{
    [SerializeField] private int hp;
    public int Hp => hp;
}
